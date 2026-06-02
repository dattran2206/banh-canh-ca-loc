import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import apiClient from '@/api/apiClient';

const DataContext = createContext(null);

export function DataProvider({ children }) {
    const [tables, setTables] = useState([]);
    const [orders, setOrders] = useState([]);
    const [categories, setCategories] = useState([]);
    const [menuItems, setMenuItems] = useState([]);
    const [ingredients, setIngredients] = useState([]);
    const [areas, setAreas] = useState([]);
    const [shopInfo, setShopInfo] = useState(null);
    const [activityLogs, setActivityLogs] = useState([]);
    const [dashboardStats, setDashboardStats] = useState(null);
    const [isLoading, setIsLoading] = useState(false);

    const token = localStorage.getItem('bcl_token');

    const refreshTables = useCallback(async () => {
        try {
            const res = await apiClient.get('/tables');
            setTables(res.data);
        } catch (err) { console.error("Error loading tables", err); }
    }, []);

    const refreshOrders = useCallback(async () => {
        try {
            const res = await apiClient.get('/orders/active');
            setOrders(res.data);
        } catch (err) { console.error("Error loading orders", err); }
    }, []);

    const refreshCategories = useCallback(async () => {
        try {
            const res = await apiClient.get('/menu/categories');
            setCategories(res.data);
        } catch (err) { console.error("Error loading categories", err); }
    }, []);

    const refreshMenuItems = useCallback(async () => {
        try {
            const res = await apiClient.get('/menu');
            setMenuItems(res.data);
        } catch (err) { console.error("Error loading menu", err); }
    }, []);

    const refreshIngredients = useCallback(async () => {
        try {
            const res = await apiClient.get('/inventory/ingredients');
            setIngredients(res.data);
        } catch (err) { console.error("Error loading ingredients", err); }
    }, []);

    const refreshAreas = useCallback(async () => {
        try {
            const res = await apiClient.get('/tables/areas');
            setAreas(res.data);
        } catch (err) { console.error("Error loading areas", err); }
    }, []);

    const refreshShopInfo = useCallback(async () => {
        try {
            const res = await apiClient.get('/shop');
            setShopInfo(res.data);
        } catch (err) { console.error("Error loading shop info", err); }
    }, []);

    const refreshActivityLogs = useCallback(async () => {
        try {
            const res = await apiClient.get('/reports/activity-logs');
            setActivityLogs(res.data);
        } catch (err) { console.error("Error loading activity logs", err); }
    }, []);

    const refreshDashboardStats = useCallback(async () => {
        try {
            const res = await apiClient.get('/reports/dashboard');
            setDashboardStats(res.data);
        } catch (err) { console.error("Error loading dashboard stats", err); }
    }, []);

    const refreshAll = useCallback(async () => {
        if (!localStorage.getItem('bcl_token')) return;
        setIsLoading(true);
        await Promise.all([
            refreshTables(),
            refreshOrders(),
            refreshCategories(),
            refreshMenuItems(),
            refreshIngredients(),
            refreshAreas(),
            refreshShopInfo(),
            refreshDashboardStats(),
        ]);
        setIsLoading(false);
    }, [refreshTables, refreshOrders, refreshCategories, refreshMenuItems, refreshIngredients, refreshAreas, refreshShopInfo, refreshDashboardStats]);

    // Load data on start if authenticated
    useEffect(() => {
        if (token) {
            refreshAll();
        }
    }, [token, refreshAll]);

    // Setup SignalR connection
    useEffect(() => {
        if (!token) return;

        const connection = new HubConnectionBuilder()
            .withUrl('http://localhost:5277/hub/orders', {
                accessTokenFactory: () => localStorage.getItem('bcl_token')
            })
            .withAutomaticReconnect()
            .build();

        connection.on('OrderUpdated', (updatedOrder) => {
            // Update order in list
            setOrders(prev => {
                if (updatedOrder.status === 'paid') {
                    return prev.filter(o => o.id !== updatedOrder.id);
                }
                const exists = prev.some(o => o.id === updatedOrder.id);
                if (exists) {
                    return prev.map(o => o.id === updatedOrder.id ? updatedOrder : o);
                } else {
                    return [updatedOrder, ...prev];
                }
            });

            // Re-fetch stats, tables, ingredients to sync
            refreshTables();
            refreshDashboardStats();
            refreshIngredients();
        });

        connection.start()
            .catch(err => console.error("SignalR Connection Error: ", err));

        return () => {
            connection.stop();
        };
    }, [token, refreshTables, refreshDashboardStats, refreshIngredients]);

    // Actions
    const addTable = async (tableData) => {
        const res = await apiClient.post('/tables', tableData);
        await refreshTables();
        return res.data;
    };

    const updateTable = async (id, tableData) => {
        const res = await apiClient.put(`/tables/${id}`, tableData);
        await refreshTables();
        return res.data;
    };

    const deleteTable = async (id) => {
        const res = await apiClient.delete(`/tables/${id}`);
        await refreshTables();
        return res.data;
    };

    const addArea = async (areaData) => {
        const res = await apiClient.post('/tables/areas', areaData);
        await refreshAreas();
        return res.data;
    };

    const deleteArea = async (id) => {
        const res = await apiClient.delete(`/tables/areas/${id}`);
        await refreshAreas();
        return res.data;
    };

    const addMenuItem = async (menuItemData) => {
        const res = await apiClient.post('/menu', menuItemData);
        await refreshMenuItems();
        return res.data;
    };

    const updateMenuItem = async (id, menuItemData) => {
        const res = await apiClient.put(`/menu/${id}`, menuItemData);
        await refreshMenuItems();
        return res.data;
    };

    const deleteMenuItem = async (id) => {
        const res = await apiClient.delete(`/menu/${id}`);
        await refreshMenuItems();
        return res.data;
    };

    const saveRecipeItem = async (recipeData) => {
        const res = await apiClient.post('/menu/recipes', recipeData);
        return res.data;
    };

    const deleteRecipeItem = async (menuItemId, ingredientId) => {
        const res = await apiClient.delete(`/menu/recipes/${menuItemId}/${ingredientId}`);
        return res.data;
    };

    const createOrder = async (orderData) => {
        const res = await apiClient.post('/orders', orderData);
        await refreshOrders();
        return res.data;
    };

    const appendOrderItems = async (orderId, items) => {
        const res = await apiClient.post(`/orders/${orderId}/items`, items);
        await refreshOrders();
        return res.data;
    };

    const updateOrderStatus = async (orderId, status) => {
        const res = await apiClient.put(`/orders/${orderId}/status`, { status });
        await refreshOrders();
        await refreshIngredients();
        return res.data;
    };

    const createPayment = async (paymentData) => {
        const res = await apiClient.post('/payments', paymentData);
        await refreshOrders();
        await refreshDashboardStats();
        return res.data;
    };

    const addStockEntry = async (entryData) => {
        const res = await apiClient.post('/inventory/stock-entries', entryData);
        await refreshIngredients();
        return res.data;
    };

    const addWasteRecord = async (recordData) => {
        const res = await apiClient.post('/inventory/waste-records', recordData);
        await refreshIngredients();
        return res.data;
    };

    const addStockTake = async (takeData) => {
        const res = await apiClient.post('/inventory/stock-takes', takeData);
        await refreshIngredients();
        return res.data;
    };

    const addIngredient = async (ingData) => {
        const res = await apiClient.post('/inventory/ingredients', ingData);
        await refreshIngredients();
        return res.data;
    };

    const updateIngredient = async (id, ingData) => {
        const res = await apiClient.put(`/inventory/ingredients/${id}`, ingData);
        await refreshIngredients();
        return res.data;
    };

    const deleteIngredient = async (id) => {
        const res = await apiClient.delete(`/inventory/ingredients/${id}`);
        await refreshIngredients();
        return res.data;
    };

    return (
        <DataContext.Provider value={{
            tables, orders, categories, menuItems, ingredients, areas, shopInfo, activityLogs, dashboardStats, isLoading,
            refreshAll, refreshTables, refreshOrders, refreshCategories, refreshMenuItems, refreshIngredients, refreshAreas, refreshShopInfo, refreshActivityLogs, refreshDashboardStats,
            addTable, updateTable, deleteTable, addArea, deleteArea,
            addMenuItem, updateMenuItem, deleteMenuItem, saveRecipeItem, deleteRecipeItem,
            createOrder, appendOrderItems, updateOrderStatus, createPayment,
            addStockEntry, addWasteRecord, addStockTake,
            addIngredient, updateIngredient, deleteIngredient
        }}>
            {children}
        </DataContext.Provider>
    );
}

export function useData() {
    const context = useContext(DataContext);
    if (!context) {
        throw new Error('useData must be used within a DataProvider');
    }
    return context;
}
