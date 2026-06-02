import os
import re
import glob

base_dir = r'c:\Users\phucpt\2.source-fe\src'

# 1. Fix Composables
print("--- Fixing Composables ---")
for file in glob.glob(os.path.join(base_dir, 'composables', 'use*List.js')):
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Check if state is defined outside
    if re.search(r'const state = reactive\([^)]+\)\s*export const use\w+List = \(\) => {', content):
        content = re.sub(
            r'(const state = reactive\([^)]+\))\s*export const (use\w+List) = \(\) => {',
            r'export const \2 = () => {\n  \1',
            content
        )
        with open(file, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f'Fixed composable: {file}')

# 2. Fix Filter Components
print("--- Fixing Filter Components ---")
filter_files = glob.glob(os.path.join(base_dir, 'pages', '**', '*Filter.vue'), recursive=True)
for file in filter_files:
    if 'OrderListFilter.vue' in file: continue # already done
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Add props if not exists
    if 'initialFilters' not in content:
        content = re.sub(
            r'(const emit = defineEmits[^;]*;?)',
            r'\1\n\nconst props = defineProps({\n  initialFilters: {\n    type: Object,\n    default: () => ({})\n  }\n})',
            content
        )
        
        # Replace route.query.xxx with props.initialFilters.xxx
        content = re.sub(r'route\.query\.(\w+)', r'props.initialFilters.\1', content)
        
        # Replace empty string initialization with props.initialFilters
        # Simple heuristic: find searchForm reactive properties
        def replacer(match):
            key = match.group(1)
            val = match.group(2)
            if "''" in val or '""' in val:
                return f'{key}: props.initialFilters.{key} || {val}'
            return match.group(0)
            
        content = re.sub(r'(\w+):\s*([^,}\n]+)', replacer, content)

        with open(file, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f'Fixed filter: {file}')

# 3. Fix List Pages
print("--- Fixing List Pages ---")
list_files = glob.glob(os.path.join(base_dir, 'pages', '**', '*List.vue'), recursive=True) + glob.glob(os.path.join(base_dir, 'pages', 'consultations', 'ConsultationPage.vue'))
for file in list_files:
    if 'OrderList.vue' in file: continue # already done
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Find filter tag
    match = re.search(r'<([A-Za-z0-9]+Filter) [^>]*@search', content)
    if match:
        tag_name = match.group(1)
        if 'initial-filters' not in content:
            content = re.sub(
                rf'(<{tag_name}\s+)([^>]*)',
                r'\1:initial-filters="state.filters"\n        \2',
                content
            )
            with open(file, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f'Fixed list page: {file}')
