import json
import openpyxl
import re
import os

RESULTS_FILE = 'test-results/test-results.json'
EXCEL_FILE = '[VENTI]_Admin_Customer Support_Testcase_VN.xlsx'

def extract_specs(suite, specs_list):
    for spec in suite.get('specs', []):
        specs_list.append(spec)
    for sub_suite in suite.get('suites', []):
        extract_specs(sub_suite, specs_list)

def sync_results():
    if not os.path.exists(RESULTS_FILE):
        print(f"Error: {RESULTS_FILE} not found.")
        return

    with open(RESULTS_FILE, 'r', encoding='utf-8') as f:
        data = json.load(f)

    test_outcomes = {}
    all_specs = []
    
    for suite in data.get('suites', []):
        extract_specs(suite, all_specs)

    for spec in all_specs:
        title = spec.get('title', '')
        # Extract tags like [ADM9_1]
        match = re.search(r'\[(.*?)\]', title)
        if match:
            tag = match.group(1)
            tests = spec.get('tests', [])
            if tests:
                results = tests[0].get('results', [])
                if results:
                    status = results[0].get('status')
                    test_outcomes[tag] = 'Pass' if status in ['expected', 'passed'] else 'Fail'

    print(f"Parsed outcomes from JSON: {test_outcomes}")

    try:
        wb = openpyxl.load_workbook(EXCEL_FILE)
    except Exception as e:
        print(f"Failed to load excel file: {e}")
        return

    ws = wb['ADM4 Customer Support']
    
    # We found that ADM9_1 starts from row 14 and matches non-empty column F (Expected Output)
    test_id_counter = 1
    updated_count = 0
    
    # Iterate from row 14 downwards
    for row in ws.iter_rows(min_row=14, max_col=10):
        expected_output_cell = row[5].value
        if expected_output_cell and str(expected_output_cell).strip() != '':
            current_tag = f"ADM9_{test_id_counter}"
            outcome = test_outcomes.get(current_tag)
            if outcome:
                row[9].value = outcome # Round 1 column
                updated_count += 1
            test_id_counter += 1

    wb.save(EXCEL_FILE)
    print(f"Sync complete. {updated_count} rows updated in Excel file.")

if __name__ == '__main__':
    sync_results()
