import { QueryClientProvider } from '@tanstack/react-query'
import { queryClientInstance } from '@/lib/query-client'
import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import { AppAuthProvider, useAppAuth } from '@/lib/appAuth';
import { initializeSeedData } from '@/lib/seedData';
import { useEffect, useState } from 'react';

import Login from './pages/Login';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import Tables from './pages/Tables';
import Orders from './pages/Orders';
import Kitchen from './pages/Kitchen';
import Menu from './pages/Menu';
import Inventory from './pages/Inventory';
import Reports from './pages/Reports';
import Staff from './pages/Staff';
import TablesManage from './pages/TablesManage';
import ShiftGuard from './components/ShiftGuard';
import { Toaster } from "@/components/ui/toaster";

function AppRoutes() {
    const { currentUser, isLoading } = useAppAuth();
    const [seeded, setSeeded] = useState(false);

    useEffect(() => {
        initializeSeedData().then(() => setSeeded(true));
    }, []);

    if (isLoading || !seeded) {
        return (
            <div className="fixed inset-0 flex items-center justify-center bg-amber-50">
                <div className="text-center">
                    <div className="w-10 h-10 border-4 border-amber-200 border-t-amber-500 rounded-full animate-spin mx-auto mb-3"></div>
                    <p className="text-amber-700 text-sm">Đang khởi động...</p>
                </div>
            </div>
        );
    }

    if (!currentUser) {
        return <Login />;
    }

    // Kitchen gets its own full-screen layout
    if (currentUser.role === 'kitchen') {
        return (
            <Routes>
                <Route path="*" element={<Kitchen />} />
            </Routes>
        );
    }

    return (
        <ShiftGuard>
            <Routes>
                <Route element={<Layout />}>
                    {currentUser.role === 'admin' && (
                        <>
                            <Route path="/" element={<Dashboard />} />
                            <Route path="/tables" element={<Tables />} />
                            <Route path="/orders" element={<Orders />} />
                            <Route path="/menu" element={<Menu />} />
                            <Route path="/inventory" element={<Inventory />} />
                            <Route path="/reports" element={<Reports />} />
                            <Route path="/staff" element={<Staff />} />
                            <Route path="/tables/manage" element={<TablesManage />} />
                        </>
                    )}
                    {currentUser.role === 'cashier' && (
                        <>
                            <Route path="/" element={<Navigate to="/tables" replace />} />
                            <Route path="/tables" element={<Tables />} />
                            <Route path="/orders" element={<Orders />} />
                            <Route path="/reports" element={<Reports />} />
                        </>
                    )}
                    {currentUser.role === 'waiter' && (
                        <>
                            <Route path="/" element={<Navigate to="/tables" replace />} />
                            <Route path="/tables" element={<Tables />} />
                            <Route path="/orders" element={<Orders />} />
                        </>
                    )}
                    <Route path="*" element={<Navigate to="/" replace />} />
                </Route>
            </Routes>
        </ShiftGuard>
    );
}

function App() {
    return (
        <AppAuthProvider>
            <QueryClientProvider client={queryClientInstance}>
                <Router>
                    <AppRoutes />
                </Router>
                <Toaster />
            </QueryClientProvider>
        </AppAuthProvider>
    );
}

export default App;