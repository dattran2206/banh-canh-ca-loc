import subprocess
import sys
import os
import argparse

# Ensure output is UTF-8 to handle emojis in Windows terminal if supported, 
# but for maximum compatibility we'll use standard text or safe characters.
if sys.platform == "win32":
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def run_command(command):
    """Runs a shell command and returns the output."""
    try:
        result = subprocess.run(
            command, 
            capture_output=True, 
            text=True, 
            shell=True, 
            check=True
        )
        return result.stdout.strip()
    except subprocess.CalledProcessError:
        return None

def main():
    parser = argparse.ArgumentParser(description="Git Cleanup Ignored Files")
    parser.add_argument("--yes", action="store_true", help="Skip confirmation prompt")
    args = parser.parse_args()

    print("[SEARCH] Searching for tracked files that should be ignored...")
    
    cmd = "git ls-files -i -c --exclude-standard"
    output = run_command(cmd)
    
    if not output:
        print("[SUCCESS] No tracked files found that match your .gitignore rules.")
        return

    files_to_remove = output.splitlines()
    count = len(files_to_remove)
    
    print(f"[WARNING] Found {count} files that are currently tracked but should be ignored:")
    for f in files_to_remove[:10]:
        print(f"  - {f}")
    if count > 10:
        print(f"  ... and {count - 10} more.")

    if args.yes:
        confirm = 'y'
    else:
        try:
            print(f"\nDo you want to remove these {count} files from Git tracking? (y/n): ", end="", flush=True)
            confirm = sys.stdin.readline().strip().lower()
        except EOFError:
            confirm = 'n'
    
    if confirm == 'y':
        print("\n[ACTION] Removing files from Git cache (physical files will be kept)...")
        for file_path in files_to_remove:
            subprocess.run(f'git rm --cached "{file_path}"', shell=True, capture_output=True)
        
        print(f"\n[SUCCESS] Successfully removed {count} files from Git cache.")
        print("Tip: You should now commit these changes to finalize the fix.")
    else:
        print("\n[CANCELLED] Operation cancelled.")

if __name__ == "__main__":
    main()
