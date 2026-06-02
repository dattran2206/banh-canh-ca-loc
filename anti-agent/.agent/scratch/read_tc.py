import pandas as pd
import json

file_path = '[VENTI]_Admin_Order Management_Testcase_VN.xlsx'
xls = pd.ExcelFile(file_path)

sheets_to_check = ['ADM5 Order - Subscription', 'Order - Subscription Details']

output = {}
for sheet in sheets_to_check:
    df = pd.read_excel(xls, sheet_name=sheet, nrows=20)
    # Convert dataframe to a list of dicts, replacing NaN with None
    output[sheet] = df.where(pd.notnull(df), None).to_dict(orient='records')

with open('.agent/scratch/tc_preview.json', 'w', encoding='utf-8') as f:
    json.dump(output, f, ensure_ascii=False, indent=2)
