import React, { useState } from 'react';
import { Outlet, Link, useLocation } from 'react-router-dom';
import { useAppAuth } from '@/lib/appAuth';
import { Button } from '@/components/ui/button';
import {
    LayoutDashboard, UtensilsCrossed, MapPin, ShoppingBag,
    Package, BarChart3, Users, LogOut, Menu, Clock, Settings
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { 
    AlertDialog, 
    AlertDialogContent, 
    AlertDialogHeader, 
    AlertDialogTitle, 
    AlertDialogDescription, 
    AlertDialogFooter, 
    AlertDialogAction, 
    AlertDialogCancel 
} from '@/components/ui/alert-dialog';

const navByRole = {
    admin: [
        { path: '/', label: 'Dashboard', icon: LayoutDashboard },
        { path: '/tables', label: 'Sơ đồ bàn', icon: MapPin },
        { path: '/orders', label: 'Order', icon: ShoppingBag },
        { path: '/menu', label: 'Thực đơn', icon: UtensilsCrossed },
        { path: '/inventory', label: 'Kho nguyên liệu', icon: Package },
        { path: '/reports', label: 'Báo cáo', icon: BarChart3 },
        { path: '/staff', label: 'Nhân viên', icon: Users },
        { path: '/tables/manage', label: 'Quản lý bàn & KV', icon: Settings },
    ],
    cashier: [
        { path: '/tables', label: 'Sơ đồ bàn', icon: MapPin },
        { path: '/orders', label: 'Thanh toán', icon: ShoppingBag },
        { path: '/reports', label: 'Báo cáo ca', icon: BarChart3 },
    ],
    waiter: [
        { path: '/tables', label: 'Sơ đồ bàn', icon: MapPin },
        { path: '/orders', label: 'Order', icon: ShoppingBag },
    ],
};

export default function Layout() {
    const { currentUser, currentShift, endShift, logout } = useAppAuth();
    const location = useLocation();
    const [sidebarOpen, setSidebarOpen] = useState(false);
    const [showEndShiftConfirm, setShowEndShiftConfirm] = useState(false);
    const [showLogoutConfirm, setShowLogoutConfirm] = useState(false);

    const navItems = navByRole[currentUser?.role] || [];

    const handleEndShift = () => {
        setShowEndShiftConfirm(true);
    };

    const handleLogout = () => {
        setShowLogoutConfirm(true);
    };

    const roleLabel = { admin: 'Chủ quán', cashier: 'Thu ngân', waiter: 'Bồi bàn', kitchen: 'Bếp' };

    return (
        <div className="flex h-screen bg-amber-50 overflow-hidden">
            {/* Mobile overlay */}
            {sidebarOpen && (
                <div className="fixed inset-0 bg-black/40 z-20 lg:hidden" onClick={() => setSidebarOpen(false)} />
            )}

            {/* Sidebar */}
            <aside className={cn(
                'fixed lg:static inset-y-0 left-0 z-30 w-64 bg-amber-900 text-amber-50 flex flex-col transition-transform duration-200',
                sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
            )}>
                <div className="p-4 border-b border-amber-800">
                    <div className="flex items-center gap-3">
                        <img src="/banh_canh_logo.png" alt="Logo" className="w-9 h-9 rounded-xl object-cover border border-amber-800" />
                        <div>
                            <p className="font-bold text-sm">Bánh Canh Cá Lóc</p>
                            <p className="text-amber-400 text-xs">{currentUser?.fullName}</p>
                        </div>
                    </div>
                    <div className="mt-2 px-2 py-1 bg-amber-800 rounded-lg text-xs text-amber-300">
                        {roleLabel[currentUser?.role]}
                    </div>
                </div>

                <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
                    {navItems.map(({ path, label, icon: Icon }) => (
                        <Link
                            key={path}
                            to={path}
                            onClick={() => setSidebarOpen(false)}
                            className={cn(
                                'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-colors',
                                location.pathname === path
                                    ? 'bg-amber-500 text-white'
                                    : 'text-amber-200 hover:bg-amber-800 hover:text-white'
                            )}
                        >
                            <Icon className="w-4 h-4 flex-shrink-0" />
                            {label}
                        </Link>
                    ))}
                </nav>

                <div className="p-3 border-t border-amber-800 space-y-2">
                    {currentShift && (
                        <Button
                            onClick={handleEndShift}
                            variant="outline"
                            size="sm"
                            className="w-full border-amber-700 text-amber-200 hover:bg-amber-800 text-xs"
                        >
                            <Clock className="w-3 h-3 mr-1" /> Kết thúc ca
                        </Button>
                    )}
                    <Button
                        onClick={handleLogout}
                        variant="ghost"
                        size="sm"
                        className="w-full text-amber-300 hover:text-white hover:bg-amber-800 text-xs"
                    >
                        <LogOut className="w-3 h-3 mr-1" /> Đăng xuất
                    </Button>
                </div>
            </aside>

            {/* Main content */}
            <div className="flex-1 flex flex-col overflow-hidden">
                <header className="lg:hidden bg-amber-900 text-white px-4 py-3 flex items-center gap-3">
                    <Button variant="ghost" size="icon" onClick={() => setSidebarOpen(true)} className="text-white hover:bg-amber-800">
                        <Menu className="w-5 h-5" />
                    </Button>
                    <span className="font-bold">Bánh Canh Cá Lóc</span>
                </header>
                <main className="flex-1 overflow-auto">
                    <Outlet />
                </main>
            </div>

            {showEndShiftConfirm && (
                <AlertDialog open={true} onOpenChange={(open) => !open && setShowEndShiftConfirm(false)}>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle className="text-amber-900">Xác nhận kết thúc ca</AlertDialogTitle>
                            <AlertDialogDescription>
                                Bạn có chắc chắn muốn kết thúc ca làm việc hiện tại?
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                            <AlertDialogCancel className="border-amber-200 text-amber-800">Hủy</AlertDialogCancel>
                            <AlertDialogAction 
                                onClick={() => { setShowEndShiftConfirm(false); endShift(); }}
                                className="bg-amber-500 hover:bg-amber-600 text-white"
                            >
                                Kết thúc ca
                            </AlertDialogAction>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            )}

            {showLogoutConfirm && (
                <AlertDialog open={true} onOpenChange={(open) => !open && setShowLogoutConfirm(false)}>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle className="text-amber-900">Xác nhận đăng xuất</AlertDialogTitle>
                            <AlertDialogDescription>
                                Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                            <AlertDialogCancel className="border-amber-200 text-amber-800">Hủy</AlertDialogCancel>
                            <AlertDialogAction 
                                onClick={() => { setShowLogoutConfirm(false); logout(); }}
                                className="bg-amber-500 hover:bg-amber-600 text-white"
                            >
                                Đăng xuất
                            </AlertDialogAction>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            )}
        </div>
    );
}