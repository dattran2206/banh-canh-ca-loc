import React, { useEffect } from 'react';
import { useData } from '@/lib/DataContext';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { AlertTriangle, TrendingUp, ShoppingBag, MapPin } from 'lucide-react';
import { Link } from 'react-router-dom';
import { format } from 'date-fns';

export default function Dashboard() {
    const { dashboardStats, refreshDashboardStats } = useData();

    useEffect(() => {
        refreshDashboardStats();
    }, [refreshDashboardStats]);

    if (!dashboardStats) {
        return (
            <div className="fixed inset-0 flex items-center justify-center bg-amber-50">
                <div className="text-center">
                    <div className="w-10 h-10 border-4 border-amber-200 border-t-amber-500 rounded-full animate-spin mx-auto mb-3"></div>
                    <p className="text-amber-700 text-sm">Đang tải số liệu...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="p-4 md:p-6 space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-amber-900">Dashboard</h1>
                <p className="text-amber-600 text-sm">{format(new Date(), 'EEEE, dd/MM/yyyy')}</p>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <Card className="border-amber-200 bg-gradient-to-br from-amber-50 to-orange-50">
                    <CardContent className="pt-4 pb-4">
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-amber-500 rounded-xl flex items-center justify-center">
                                <TrendingUp className="w-5 h-5 text-white" />
                            </div>
                            <div>
                                <p className="text-xs text-amber-600">Doanh thu hôm nay</p>
                                <p className="text-lg font-bold text-amber-900">
                                    {(dashboardStats.todayRevenue || 0).toLocaleString('vi-VN')}đ
                                </p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-amber-200 bg-gradient-to-br from-amber-50 to-orange-50">
                    <CardContent className="pt-4 pb-4">
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-orange-500 rounded-xl flex items-center justify-center">
                                <ShoppingBag className="w-5 h-5 text-white" />
                            </div>
                            <div>
                                <p className="text-xs text-amber-600">Hóa đơn hôm nay</p>
                                <p className="text-lg font-bold text-amber-900">{dashboardStats.todayBills || 0}</p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-amber-200 bg-gradient-to-br from-amber-50 to-orange-50">
                    <CardContent className="pt-4 pb-4">
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-yellow-500 rounded-xl flex items-center justify-center">
                                <MapPin className="w-5 h-5 text-white" />
                            </div>
                            <div>
                                <p className="text-xs text-amber-600">Bàn có khách</p>
                                <p className="text-lg font-bold text-amber-900">
                                    {dashboardStats.occupiedCount || 0}/{dashboardStats.totalTables || 0}
                                </p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-amber-200 bg-gradient-to-br from-amber-50 to-orange-50">
                    <CardContent className="pt-4 pb-4">
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-amber-700 rounded-xl flex items-center justify-center">
                                <ShoppingBag className="w-5 h-5 text-white" />
                            </div>
                            <div>
                                <p className="text-xs text-amber-600">Order đang xử lý</p>
                                <p className="text-lg font-bold text-amber-900">{dashboardStats.activeOrderCount || 0}</p>
                            </div>
                        </div>
                    </CardContent>
                </Card>
            </div>

            {/* Low stock alerts */}
            {dashboardStats.lowStock && dashboardStats.lowStock.length > 0 && (
                <Card className="border-red-200 bg-red-50">
                    <CardHeader className="pb-2">
                        <CardTitle className="text-red-700 flex items-center gap-2 text-base">
                            <AlertTriangle className="w-5 h-5" />
                            Cảnh báo tồn kho thấp ({dashboardStats.lowStock.length} nguyên liệu)
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
                            {dashboardStats.lowStock.map(ing => (
                                <div key={ing.id} className="flex items-center justify-between bg-white rounded-lg px-3 py-2 border border-red-100">
                                    <div>
                                        <p className="font-medium text-sm text-red-800">{ing.name}</p>
                                        <p className="text-xs text-red-500">Ngưỡng: {ing.minThreshold} {ing.unit}</p>
                                    </div>
                                    <Badge variant="destructive" className="text-xs">
                                        {ing.currentStock} {ing.unit}
                                    </Badge>
                                </div>
                            ))}
                        </div>
                        <Link to="/inventory" className="mt-3 inline-block text-xs text-red-600 underline">
                            Xem quản lý kho →
                        </Link>
                    </CardContent>
                </Card>
            )}
        </div>
    );
}