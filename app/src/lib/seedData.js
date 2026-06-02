import bcrypt from 'bcryptjs';
import { KEYS, set, setList, get, generateId } from './storage';

export async function initializeSeedData() {
    if (get(KEYS.INITIALIZED)) return;

    // Hash passwords
    const adminHash = await bcrypt.hash('admin123', 10);
    const waiterHash = await bcrypt.hash('waiter123', 10);
    const cashierHash = await bcrypt.hash('cashier123', 10);
    const kitchenHash = await bcrypt.hash('kitchen123', 10);

    const users = [
        { id: 'u1', username: 'admin', passwordHash: adminHash, role: 'admin', isActive: true, fullName: 'Chủ Quán' },
        { id: 'u2', username: 'waiter01', passwordHash: waiterHash, role: 'waiter', isActive: true, fullName: 'Bồi Bàn 1' },
        { id: 'u3', username: 'cashier01', passwordHash: cashierHash, role: 'cashier', isActive: true, fullName: 'Thu Ngân 1' },
        { id: 'u4', username: 'kitchen01', passwordHash: kitchenHash, role: 'kitchen', isActive: true, fullName: 'Bếp 1' },
    ];

    const categories = [
        { id: 'cat1', name: 'Món chính' },
        { id: 'cat2', name: 'Topping' },
        { id: 'cat3', name: 'Nước uống' },
        { id: 'cat4', name: 'Khác' },
    ];

    const menuItems = [
        { id: 'm1', name: 'Bánh canh cá lóc thường', categoryId: 'cat1', price: 45000, description: 'Tô bánh canh cá lóc size thường', isAvailable: true },
        { id: 'm2', name: 'Bánh canh cá lóc lớn', categoryId: 'cat1', price: 55000, description: 'Tô bánh canh cá lóc size lớn', isAvailable: true },
        { id: 'm3', name: 'Bánh canh cá lóc đặc biệt', categoryId: 'cat1', price: 65000, description: 'Tô đặc biệt nhiều cá, thêm trứng cút', isAvailable: true },
        { id: 'm4', name: 'Thêm cá lóc', categoryId: 'cat2', price: 20000, description: 'Thêm phần cá lóc', isAvailable: true },
        { id: 'm5', name: 'Thêm trứng cút', categoryId: 'cat2', price: 8000, description: 'Thêm 4 trứng cút', isAvailable: true },
        { id: 'm6', name: 'Thêm bánh canh', categoryId: 'cat2', price: 10000, description: 'Thêm phần bánh canh', isAvailable: true },
        { id: 'm7', name: 'Trà đá', categoryId: 'cat3', price: 5000, description: 'Trà đá miễn phí refill', isAvailable: true },
        { id: 'm8', name: 'Nước ngọt', categoryId: 'cat3', price: 15000, description: 'Coca, Pepsi, 7UP', isAvailable: true },
        { id: 'm9', name: 'Nước suối', categoryId: 'cat3', price: 10000, description: 'Nước suối chai 500ml', isAvailable: true },
    ];

    const ingredients = [
        { id: 'ing1', name: 'Cá lóc tươi', unit: 'kg', currentStock: 10, minThreshold: 2 },
        { id: 'ing2', name: 'Bánh canh sợi', unit: 'kg', currentStock: 15, minThreshold: 3 },
        { id: 'ing3', name: 'Nước lèo', unit: 'lít', currentStock: 20, minThreshold: 5 },
        { id: 'ing4', name: 'Hành lá', unit: 'kg', currentStock: 1, minThreshold: 0.2 },
        { id: 'ing5', name: 'Rau ăn kèm', unit: 'kg', currentStock: 2, minThreshold: 0.5 },
        { id: 'ing6', name: 'Trứng cút', unit: 'cái', currentStock: 100, minThreshold: 20 },
        { id: 'ing7', name: 'Gia vị tổng hợp', unit: 'kg', currentStock: 2, minThreshold: 0.3 },
    ];

    // Recipes: menuItemId, ingredientId, quantity (per serving), yieldPercent
    const recipeItems = [
        // Bánh canh thường
        { menuItemId: 'm1', ingredientId: 'ing1', quantity: 0.15, yieldPercent: 0.7 },
        { menuItemId: 'm1', ingredientId: 'ing2', quantity: 0.1, yieldPercent: 1.0 },
        { menuItemId: 'm1', ingredientId: 'ing3', quantity: 0.5, yieldPercent: 1.0 },
        { menuItemId: 'm1', ingredientId: 'ing4', quantity: 0.01, yieldPercent: 1.0 },
        { menuItemId: 'm1', ingredientId: 'ing5', quantity: 0.05, yieldPercent: 1.0 },
        // Bánh canh lớn
        { menuItemId: 'm2', ingredientId: 'ing1', quantity: 0.2, yieldPercent: 0.7 },
        { menuItemId: 'm2', ingredientId: 'ing2', quantity: 0.13, yieldPercent: 1.0 },
        { menuItemId: 'm2', ingredientId: 'ing3', quantity: 0.65, yieldPercent: 1.0 },
        { menuItemId: 'm2', ingredientId: 'ing4', quantity: 0.012, yieldPercent: 1.0 },
        { menuItemId: 'm2', ingredientId: 'ing5', quantity: 0.06, yieldPercent: 1.0 },
        // Bánh canh đặc biệt
        { menuItemId: 'm3', ingredientId: 'ing1', quantity: 0.25, yieldPercent: 0.7 },
        { menuItemId: 'm3', ingredientId: 'ing2', quantity: 0.15, yieldPercent: 1.0 },
        { menuItemId: 'm3', ingredientId: 'ing3', quantity: 0.7, yieldPercent: 1.0 },
        { menuItemId: 'm3', ingredientId: 'ing4', quantity: 0.015, yieldPercent: 1.0 },
        { menuItemId: 'm3', ingredientId: 'ing5', quantity: 0.07, yieldPercent: 1.0 },
        { menuItemId: 'm3', ingredientId: 'ing6', quantity: 4, yieldPercent: 1.0 },
        // Topping
        { menuItemId: 'm4', ingredientId: 'ing1', quantity: 0.15, yieldPercent: 0.7 },
        { menuItemId: 'm5', ingredientId: 'ing6', quantity: 4, yieldPercent: 1.0 },
        { menuItemId: 'm6', ingredientId: 'ing2', quantity: 0.1, yieldPercent: 1.0 },
    ];

    const areas = [
        { id: 'indoor', name: 'Trong nhà' },
        { id: 'outdoor', name: 'Ngoài trời' }
    ];

    const tables = [
        { id: 't1', number: 1, area: 'indoor', capacity: 4 },
        { id: 't2', number: 2, area: 'indoor', capacity: 4 },
        { id: 't3', number: 3, area: 'indoor', capacity: 4 },
        { id: 't4', number: 4, area: 'indoor', capacity: 6 },
        { id: 't5', number: 5, area: 'indoor', capacity: 6 },
        { id: 't6', number: 6, area: 'outdoor', capacity: 4 },
        { id: 't7', number: 7, area: 'outdoor', capacity: 4 },
        { id: 't8', number: 8, area: 'outdoor', capacity: 6 },
        { id: 't9', number: 9, area: 'outdoor', capacity: 2 },
        { id: 't10', number: 10, area: 'outdoor', capacity: 2 },
    ];

    const shopInfo = {
        name: 'Quán Bánh Canh Cá Lóc',
        address: '123 Đường Lê Lợi, Q.1, TP.HCM',
        phone: '0901 234 567',
    };

    setList(KEYS.USERS, users);
    setList(KEYS.CATEGORIES, categories);
    setList(KEYS.MENU_ITEMS, menuItems);
    setList(KEYS.INGREDIENTS, ingredients);
    setList(KEYS.RECIPE_ITEMS, recipeItems);
    setList(KEYS.AREAS, areas);
    setList(KEYS.TABLES, tables);
    setList(KEYS.ORDERS, []);
    setList(KEYS.ORDER_ITEMS, []);
    setList(KEYS.PAYMENTS, []);
    setList(KEYS.STOCK_ENTRIES, []);
    setList(KEYS.WASTE_RECORDS, []);
    setList(KEYS.STOCK_TAKES, []);
    setList(KEYS.ACTIVITY_LOGS, []);
    setList(KEYS.SHIFTS, []);
    set(KEYS.SHOP_INFO, shopInfo);
    set(KEYS.INITIALIZED, true);
}