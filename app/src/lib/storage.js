// localStorage keys
export const KEYS = {
    USERS: 'bcl_users',
    SHIFTS: 'bcl_shifts',
    TABLES: 'bcl_tables',
    AREAS: 'bcl_areas',
    CATEGORIES: 'bcl_categories',
    MENU_ITEMS: 'bcl_menu_items',
    INGREDIENTS: 'bcl_ingredients',
    RECIPE_ITEMS: 'bcl_recipe_items',
    ORDERS: 'bcl_orders',
    ORDER_ITEMS: 'bcl_order_items',
    PAYMENTS: 'bcl_payments',
    STOCK_ENTRIES: 'bcl_stock_entries',
    WASTE_RECORDS: 'bcl_waste_records',
    STOCK_TAKES: 'bcl_stock_takes',
    ACTIVITY_LOGS: 'bcl_activity_logs',
    CURRENT_USER: 'bcl_current_user',
    SHOP_INFO: 'bcl_shop_info',
    INITIALIZED: 'bcl_initialized',
};

export function get(key) {
    try {
        const val = localStorage.getItem(key);
        return val ? JSON.parse(val) : null;
    } catch { return null; }
}

export function set(key, value) {
    localStorage.setItem(key, JSON.stringify(value));
}

export function getList(key) {
    return get(key) || [];
}

export function setList(key, list) {
    set(key, list);
}

export function addToList(key, item) {
    const list = getList(key);
    list.push(item);
    setList(key, list);
    return item;
}

export function updateInList(key, id, updates) {
    const list = getList(key);
    const idx = list.findIndex(i => i.id === id);
    if (idx !== -1) {
        list[idx] = { ...list[idx], ...updates };
        setList(key, list);
        return list[idx];
    }
    return null;
}

export function removeFromList(key, id) {
    const list = getList(key).filter(i => i.id !== id);
    setList(key, list);
}

export function generateId() {
    return Date.now().toString(36) + Math.random().toString(36).substr(2, 5);
}