import sys
import subprocess
import os

def main():
    if len(sys.argv) < 2:
        print("Usage: python .agent/scripts/triage_worker.py '<topic>'")
        sys.exit(1)

    topic = sys.argv[1]
    
    # Use absolute or workspace-relative path safely
    # Assumes script is run from project root
    output_file = ".agent/tmp_wiki_context.md"

    # Ensure .agent directory exists
    os.makedirs(".agent", exist_ok=True)

    prompt = (
        f"Research the topic: '{topic}'. Scan the markdown/Wiki files in the project. "
        f"Apply SkillReducer principles: aggressively filter out examples, verbose explanations, and non-actionable fluff. "
        f"Extract ONLY the CORE RULES and CRITICAL LOGIC. "
        f"Write the compressed summary to '{output_file}' and exit."
    )

    print(f"[*] Đang khởi động Triage Worker (opencode) cho chủ đề: {topic}...")
    
    try:
        command = ["opencode", "-m", prompt]
        
        # Chạy công cụ CLI với timeout 120s để chống kẹt Terminal
        result = subprocess.run(
            command, 
            capture_output=True, 
            text=True, 
            timeout=120
        )

        # Kiểm tra file đã thực sự được sinh ra chưa
        if os.path.exists(output_file):
            print(f"[+] Thành công! Ngữ cảnh đã được nén tại: {output_file}.")
            if result.returncode != 0:
                print(f"[!] Chú ý: Worker đã ghi file nhưng thoát với mã lỗi: {result.returncode}")
        else:
            print(f"[-] Thất bại: Worker chạy xong nhưng không tìm thấy file {output_file}.")
            print("Output:\n", result.stdout)
            print("Errors:\n", result.stderr)

    except subprocess.TimeoutExpired:
        print("[-] Hủy bỏ: Triage Worker bị treo và đã tự động ngắt sau 120s.")
        sys.exit(1)
    except Exception as e:
        print(f"[-] Lỗi hệ thống khi gọi opencode: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main()
