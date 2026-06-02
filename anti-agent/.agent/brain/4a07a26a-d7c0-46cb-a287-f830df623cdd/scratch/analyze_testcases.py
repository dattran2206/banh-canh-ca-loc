import pandas as pd
import json
import sys

def analyze_excel(file_path, output_path):
    try:
        # Explicitly use openpyxl engine
        xl = pd.ExcelFile(file_path, engine='openpyxl')
        summary = {}
        
        for sheet_name in xl.sheet_names:
            df = xl.parse(sheet_name)
            # Clean up column names and take head
            df.columns = [str(c).strip() for c in df.columns]
            # Convert to list of dicts for the first 20 rows
            rows = df.head(50).to_dict(orient='records')
            summary[sheet_name] = {
                "columns": list(df.columns),
                "total_rows": len(df),
                "sample_data": rows
            }
            
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(summary, f, ensure_ascii=False, indent=2, default=str)
        print(f"Analysis saved to {output_path}")
        
    except Exception as e:
        print(f"Error: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    analyze_excel(sys.argv[1], sys.argv[2])
