import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import bcrypt from 'bcryptjs';
import { KEYS, get, set, getList, setList, addToList, updateInList, generateId } from './storage';

const AppAuthContext = createContext(null);
const SESSION_TIMEOUT_MS = 8 * 60 * 60 * 1000;

export function AppAuthProvider({ children }) {
    const [currentUser, setCurrentUser] = useState(null);
    const [currentShift, setCurrentShift] = useState(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const session = get(KEYS.CURRENT_USER);
        if (session) {
            const elapsed = Date.now() - session.lastActivity;
            if (elapsed < SESSION_TIMEOUT_MS) {
                setCurrentUser(session.user);
                setCurrentShift(session.shiftId || null);
                set(KEYS.CURRENT_USER, { ...session, lastActivity: Date.now() });
            } else {
                set(KEYS.CURRENT_USER, null);
            }
        }
        setIsLoading(false);
    }, []);

    const pingActivity = useCallback(() => {
        const session = get(KEYS.CURRENT_USER);
        if (session) set(KEYS.CURRENT_USER, { ...session, lastActivity: Date.now() });
    }, []);

    useEffect(() => {
        window.addEventListener('click', pingActivity);
        window.addEventListener('keypress', pingActivity);
        return () => {
            window.removeEventListener('click', pingActivity);
            window.removeEventListener('keypress', pingActivity);
        };
    }, [pingActivity]);

    const login = async (username, password) => {
        const users = getList(KEYS.USERS);
        const user = users.find(u => u.username === username && u.isActive);
        if (!user) throw new Error('Tài khoản không tồn tại hoặc đã bị khóa');
        const match = await bcrypt.compare(password, user.passwordHash);
        if (!match) throw new Error('Mật khẩu không đúng');
        const session = { user, lastActivity: Date.now(), shiftId: null };
        set(KEYS.CURRENT_USER, session);
        setCurrentUser(user);
        doLogActivity(user.id, 'login', 'Đăng nhập');
        return user;
    };

    const logout = () => {
        const session = get(KEYS.CURRENT_USER);
        if (session?.user) doLogActivity(session.user.id, 'logout', 'Đăng xuất');
        set(KEYS.CURRENT_USER, null);
        setCurrentUser(null);
        setCurrentShift(null);
    };

    const startShift = () => {
        if (!currentUser) return null;
        const shift = {
            id: generateId(),
            userId: currentUser.id,
            startTime: new Date().toISOString(),
            endTime: null,
            totalRevenue: 0,
            totalBills: 0,
        };
        addToList(KEYS.SHIFTS, shift);
        const session = get(KEYS.CURRENT_USER);
        set(KEYS.CURRENT_USER, { ...session, shiftId: shift.id });
        setCurrentShift(shift.id);
        doLogActivity(currentUser.id, 'start_shift', 'Bắt đầu ca làm việc');
        return shift;
    };

    const endShift = () => {
        if (!currentShift || !currentUser) return;
        const shifts = getList(KEYS.SHIFTS);
        const payments = getList(KEYS.PAYMENTS);
        const orders = getList(KEYS.ORDERS);
        const shiftPayments = payments.filter(p => {
            const order = orders.find(o => o.id === p.orderId);
            return order?.shiftId === currentShift;
        });
        const totalRevenue = shiftPayments.reduce((s, p) => s + p.totalAmount, 0);
        const totalBills = shiftPayments.length;
        const idx = shifts.findIndex(s => s.id === currentShift);
        if (idx !== -1) {
            shifts[idx] = { ...shifts[idx], endTime: new Date().toISOString(), totalRevenue, totalBills };
            setList(KEYS.SHIFTS, shifts);
        }
        doLogActivity(currentUser.id, 'end_shift', 'Kết thúc ca làm việc');
        const session = get(KEYS.CURRENT_USER);
        set(KEYS.CURRENT_USER, { ...session, shiftId: null });
        setCurrentShift(null);
    };

    return (
        <AppAuthContext.Provider value={{ currentUser, currentShift, isLoading, login, logout, startShift, endShift }}>
            {children}
        </AppAuthContext.Provider>
    );
}

export function useAppAuth() {
    return useContext(AppAuthContext);
}

export function doLogActivity(userId, action, detail) {
    addToList(KEYS.ACTIVITY_LOGS, {
        id: generateId(),
        userId,
        action,
        detail,
        timestamp: new Date().toISOString(),
    });
}