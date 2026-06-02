import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import apiClient from '@/api/apiClient';

const AppAuthContext = createContext(null);

export function AppAuthProvider({ children }) {
    const [currentUser, setCurrentUser] = useState(null);
    const [currentShift, setCurrentShift] = useState(null);
    const [isLoading, setIsLoading] = useState(true);

    // Verify session on mount
    useEffect(() => {
        const token = localStorage.getItem('bcl_token');
        if (token) {
            apiClient.get('/auth/me')
                .then(res => {
                    const { user, shiftId } = res.data;
                    setCurrentUser(user);
                    setCurrentShift(shiftId);
                })
                .catch(err => {
                    console.error("Session expired or invalid", err);
                    localStorage.removeItem('bcl_token');
                    localStorage.removeItem('bcl_current_user');
                    setCurrentUser(null);
                    setCurrentShift(null);
                })
                .finally(() => {
                    setIsLoading(false);
                });
        } else {
            setIsLoading(false);
        }
    }, []);

    const login = async (username, password) => {
        const res = await apiClient.post('/auth/login', { username, password });
        const { token, user } = res.data;
        localStorage.setItem('bcl_token', token);
        localStorage.setItem('bcl_current_user', JSON.stringify(user));
        setCurrentUser(user);

        // Fetch shift status
        try {
            const meRes = await apiClient.get('/auth/me');
            setCurrentShift(meRes.data.shiftId);
        } catch {
            setCurrentShift(null);
        }
        return user;
    };

    const logout = () => {
        localStorage.removeItem('bcl_token');
        localStorage.removeItem('bcl_current_user');
        setCurrentUser(null);
        setCurrentShift(null);
    };

    const startShift = async () => {
        const res = await apiClient.post('/auth/shift/start');
        const shift = res.data;
        setCurrentShift(shift.id);
        return shift;
    };

    const endShift = async () => {
        const res = await apiClient.post('/auth/shift/end');
        setCurrentShift(null);
        return res.data;
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
    // Log activity is now handled automatically on the backend server for security and traceability.
    // We leave this mock here so that legacy components do not throw undefined reference errors.
    apiClient.post('/reports/activity-logs', { action, detail }).catch(() => {});
}