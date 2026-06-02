import subprocess
import sys
import os
import argparse

# Ensure output is UTF-8
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
    except subprocess.CalledProcessError as e:
        print(f"[ERROR] Command failed: {e}")
        return None

def list_skipped():
    print("[LIST] Files currently marked as 'skip-worktree':")
    # In git ls-files -v, 'S' denotes skip-worktree
    cmd = "git ls-files -v"
    output = run_command(cmd)
    
    if output:
        skipped = [line[2:] for line in output.splitlines() if line.startswith('S')]
        if skipped:
            for f in skipped:
                print(f"  - {f}")
        else:
            print("  (None)")
    else:
        print("  (None)")

def main():
    parser = argparse.ArgumentParser(description="Git Skip Worktree Utility")
    parser.add_argument("--skip", help="File to skip local changes for")
    parser.add_argument("--unskip", help="File to stop skipping local changes for")
    parser.add_argument("--list", action="store_true", help="List all skipped files")
    
    args = parser.parse_args()

    if args.list:
        list_skipped()
        return

    if args.skip:
        path = args.skip
        if not os.path.exists(path):
            print(f"[ERROR] Path does not exist: {path}")
            return
        
        print(f"[ACTION] Skipping worktree for: {path}")
        run_command(f'git update-index --skip-worktree "{path}"')
        print("[SUCCESS] Git will now ignore local changes to this file.")
        return

    if args.unskip:
        path = args.unskip
        print(f"[ACTION] Restoring worktree for: {path}")
        run_command(f'git update-index --no-skip-worktree "{path}"')
        print("[SUCCESS] Git will now track changes to this file normally.")
        return

    parser.print_help()

if __name__ == "__main__":
    main()
