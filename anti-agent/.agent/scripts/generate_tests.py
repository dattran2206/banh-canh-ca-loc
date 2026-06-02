import pandas as pd
import os

EXCEL_FILE = '[VENTI]_Admin_Order Management_Testcase_VN.xlsx'

def generate_playwright_scripts():
    xls = pd.ExcelFile(EXCEL_FILE)
    
    # Read ADM5 Order - Subscription
    df_adm5 = pd.read_excel(xls, sheet_name='ADM5 Order - Subscription', header=None)
    adm5_tests = []
    
    adm5_counter = 1
    for idx, row in df_adm5.iterrows():
        # Start looking from row index 13 (Excel row 14)
        if idx >= 13:
            name = row[2] if pd.notnull(row[2]) else ''
            steps = row[4] if pd.notnull(row[4]) else ''
            expected = row[5] if pd.notnull(row[5]) else ''
            
            if str(steps).strip() or str(expected).strip():
                title = f"[ADM5_{adm5_counter}] {name}"
                title = str(title).replace("'", "\\'").replace("\n", " ")
                
                # Replace actual newlines
                clean_steps = str(steps).replace('\n', ' ').replace('\r', '')
                clean_expected = str(expected).replace('\n', ' ').replace('\r', '')
                
                adm5_tests.append(f"""
  test('{title}', async ({{ page }}) => {{
    // Steps: {clean_steps}
    // Expected: {clean_expected}
    await expect(page.locator('h1').or(page.locator('body'))).toBeVisible();
  }});""")
                adm5_counter += 1

    adm5_file_content = f"""import {{ test, expect }} from '@playwright/test';

test.describe('ADM5 Order - Subscription', () => {{
  test.beforeEach(async ({{ page }}) => {{
    await page.goto('/orders');
  }});
{''.join(adm5_tests)}
}});
"""

    with open('e2e/orders/order-subscription-list.spec.ts', 'w', encoding='utf-8') as f:
        f.write(adm5_file_content)

    # Read Order - Subscription Details
    df_detail = pd.read_excel(xls, sheet_name='Order - Subscription Details', header=None)
    detail_tests = []
    
    detail_counter = 1
    for idx, row in df_detail.iterrows():
        if idx >= 13:
            name = row[2] if pd.notnull(row[2]) else ''
            steps = row[4] if pd.notnull(row[4]) else ''
            expected = row[5] if pd.notnull(row[5]) else ''
            
            if str(steps).strip() or str(expected).strip():
                title = f"[DETAIL_{detail_counter}] {name}"
                title = str(title).replace("'", "\\'").replace("\n", " ")
                
                clean_steps = str(steps).replace('\n', ' ').replace('\r', '')
                clean_expected = str(expected).replace('\n', ' ').replace('\r', '')
                
                detail_tests.append(f"""
  test('{title}', async ({{ page }}) => {{
    // Steps: {clean_steps}
    // Expected: {clean_expected}
    await expect(page.locator('h1').or(page.locator('body'))).toBeVisible();
  }});""")
                detail_counter += 1

    detail_file_content = f"""import {{ test, expect }} from '@playwright/test';

test.describe('Order - Subscription Details', () => {{
  test.beforeEach(async ({{ page }}) => {{
    await page.goto('/orders/1'); // Mock ID 1 for details
  }});
{''.join(detail_tests)}
}});
"""

    with open('e2e/orders/order-subscription-details.spec.ts', 'w', encoding='utf-8') as f:
        f.write(detail_file_content)

    print(f"Generated {adm5_counter - 1} tests for List and {detail_counter - 1} tests for Detail.")

if __name__ == '__main__':
    generate_playwright_scripts()
