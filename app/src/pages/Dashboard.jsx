import React, { useMemo } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { getList, KEYS } from '@/lib/storage';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { AlertTriangle, TrendingUp, ShoppingBag, MapPin } from 'lucide-react';
import { Link } from 'react-router-dom';
import { format } from 'date-fns';

export default function Dashboard() {
    const { currentUser } = useAppAuth();

    const data = useMemo(() => {
        const orders = getList(KEYS.ORDERS);
        const payments = getList(KEYS.PAYMENTS);
        const tables = getList(KEYS.TABLES);
        const ingredients = getList(KEYS.INGREDIENTS);

        const today = format(new Date(), 'yyyy-MM-dd');
        const todayPayments = payments.filter(p => p.paidAt?.startsWith(today));
        const todayRevenue = todayPayments.reduce((s, p) => s + p.totalAmount, 0);

        const activeOrders = orders.filter(o => ['pending', 'confirmed', 'preparing', 'ready'].includes(o.status));
        const lowStock = ingredients.filter(i => i.currentStock <= i.minThreshold);

        // Table statuses
        const tableStatuses = tables.map(t => {
            const tOrders = activeOrders.filter(o => o.tableId === t.id);
            let status = 'empty';
            if (tOrders.length > 0) status = 'occupied';
            return { ...t, status, orderCount: tOrders.length };
        });

        const occupiedCount = tableStatuses.filter(t => t.status === 'occupied').length;

        return { todayRevenue, todayBills: todayPayments.length, activeOrderCount: activeOrders.length, occupiedCount, totalTables: tables.length, lowStock };
    }, []);

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
                                    {data.todayRevenue.toLocaleString('vi-VN')}đ
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
                                <p className="text-lg font-bold text-amber-900">{data.todayBills}</p>
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
                                <p className="text-lg font-bold text-amber-900">{data.occupiedCount}/{data.totalTables}</p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-amber-200 bg-gradient-to-br from-amber-50 to-orange-50">
                    <CardContent className="pt-4 pb-4">
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-brown-500 bg-amber-700 rounded-xl flex items-center justify-center">
                                <ShoppingBag className="w-5 h-5 text-white" />
                            </div>
                            <div>
                                <p className="text-xs text-amber-600">Order đang xử lý</p>
                                <p className="text-lg font-bold text-amber-900">{data.activeOrderCount}</p>
                            </div>
                        </div>
                    </CardContent>
                </Card>
            </div>

            {/* Low stock alerts */}
            {data.lowStock.length > 0 && (
                <Card className="border-red-200 bg-red-50">
                    <CardHeader className="pb-2">
                        <CardTitle className="text-red-700 flex items-center gap-2 text-base">
                            <AlertTriangle className="w-5 h-5" />
                            Cảnh báo tồn kho thấp ({data.lowStock.length} nguyên liệu)
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
                            {data.lowStock.map(ing => (
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