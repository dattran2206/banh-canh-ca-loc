import React, { useMemo } from 'react';
import { getList, KEYS } from '@/lib/storage';
import { useAppAuth } from '@/lib/appAuth';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import { TrendingUp, TrendingDown, Minus } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import { format, startOfDay, endOfDay, startOfWeek, endOfWeek, startOfMonth, endOfMonth, subDays, subWeeks, subMonths } from 'date-fns';

function getRevenue(payments, from, to) {
    return payments.filter(p => {
        const d = new Date(p.paidAt);
        return d >= from && d <= to;
    });
}

function CompareCard({ label, current, previous }) {
    const diff = current - previous;
    const pct = previous > 0 ? Math.round((diff / previous) * 100) : (current > 0 ? 100 : 0);
    const isUp = diff > 0;
    const isDown = diff < 0;
    return (
        <Card className="border-amber-200">
            <CardContent className="pt-4 pb-4">
                <p className="text-xs text-amber-600 mb-1">{label}</p>
                <p className="text-2xl font-bold text-amber-900">{typeof current === 'number' && current > 999 ? `${current.toLocaleString('vi-VN')}đ` : current}</p>
                <div className="flex items-center gap-1 mt-1">
                    {isUp && <TrendingUp className="w-4 h-4 text-green-500" />}
                    {isDown && <TrendingDown className="w-4 h-4 text-red-500" />}
                    {!isUp && !isDown && <Minus className="w-4 h-4 text-gray-400" />}
                    <span className={`text-sm font-medium ${isUp ? 'text-green-600' : isDown ? 'text-red-600' : 'text-gray-500'}`}>
                        {isUp ? '+' : ''}{pct}% so với kỳ trước
                    </span>
                </div>
            </CardContent>
        </Card>
    );
}

export default function Reports() {
    const { currentUser, currentShift } = useAppAuth();

    const { payments, orders, users, shifts } = useMemo(() => ({
        payments: getList(KEYS.PAYMENTS),
        orders: getList(KEYS.ORDERS),
        users: getList(KEYS.USERS),
        shifts: getList(KEYS.SHIFTS),
    }), []);

    const now = new Date();

    const periodData = useMemo(() => {
        // Today vs yesterday
        const todayPayments = getRevenue(payments, startOfDay(now), endOfDay(now));
        const yestPayments = getRevenue(payments, startOfDay(subDays(now, 1)), endOfDay(subDays(now, 1)));

        // This week vs last week
        const thisWeekPayments = getRevenue(payments, startOfWeek(now, { weekStartsOn: 1 }), endOfWeek(now, { weekStartsOn: 1 }));
        const lastWeekPayments = getRevenue(payments, startOfWeek(subWeeks(now, 1), { weekStartsOn: 1 }), endOfWeek(subWeeks(now, 1), { weekStartsOn: 1 }));

        // This month vs last month
        const thisMonthPayments = getRevenue(payments, startOfMonth(now), endOfMonth(now));
        const lastMonthPayments = getRevenue(payments, startOfMonth(subMonths(now, 1)), endOfMonth(subMonths(now, 1)));

        return { todayPayments, yestPayments, thisWeekPayments, lastWeekPayments, thisMonthPayments, lastMonthPayments };
    }, [payments]);

    // Top selling items
    const topItems = useMemo(() => {
        const orderItems = getList(KEYS.ORDER_ITEMS);
        const menuItems = getList(KEYS.MENU_ITEMS);
        const counts = {};
        orderItems.forEach(oi => {
            counts[oi.menuItemId] = (counts[oi.menuItemId] || 0) + oi.quantity;
        });
        return Object.entries(counts)
            .sort((a, b) => b[1] - a[1])
            .slice(0, 5)
            .map(([id, qty]) => ({ name: menuItems.find(m => m.id === id)?.name || id, qty }));
    }, []);

    // Daily chart - last 7 days
    const dailyChart = useMemo(() => {
        return Array.from({ length: 7 }, (_, i) => {
            const day = subDays(now, 6 - i);
            const dayPayments = getRevenue(payments, startOfDay(day), endOfDay(day));
            return {
                name: format(day, 'dd/MM'),
                revenue: dayPayments.reduce((s, p) => s + p.totalAmount, 0),
                bills: dayPayments.length,
            };
        });
    }, [payments]);

    // Shifts table (cashier sees own, admin sees all)
    const visibleShifts = currentUser?.role === 'admin'
        ? shifts.slice(-20).reverse()
        : shifts.filter(s => s.userId === currentUser?.id).slice(-10).reverse();

    const sum = (arr) => arr.reduce((s, p) => s + p.totalAmount, 0);

    return (
        <div className="p-4 md:p-6 space-y-6">
            <h1 className="text-2xl font-bold text-amber-900">Báo cáo doanh thu</h1>

            <Tabs defaultValue="daily">
                <TabsList className="bg-amber-100">
                    <TabsTrigger value="daily" className="data-[state=active]:bg-amber-500 data-[state=active]:text-white">Ngày</TabsTrigger>
                    <TabsTrigger value="weekly" className="data-[state=active]:bg-amber-500 data-[state=active]:text-white">Tuần</TabsTrigger>
                    <TabsTrigger value="monthly" className="data-[state=active]:bg-amber-500 data-[state=active]:text-white">Tháng</TabsTrigger>
                    <TabsTrigger value="shifts" className="data-[state=active]:bg-amber-500 data-[state=active]:text-white">Ca làm việc</TabsTrigger>
                </TabsList>

                <TabsContent value="daily" className="space-y-4 mt-4">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <CompareCard label="Doanh thu hôm nay" current={sum(periodData.todayPayments)} previous={sum(periodData.yestPayments)} />
                        <CompareCard label="Số hóa đơn hôm nay" current={periodData.todayPayments.length} previous={periodData.yestPayments.length} />
                    </div>
                    <Card className="border-amber-200">
                        <CardHeader className="pb-2"><CardTitle className="text-base text-amber-900">Doanh thu 7 ngày qua</CardTitle></CardHeader>
                        <CardContent>
                            <ResponsiveContainer width="100%" height={200}>
                                <BarChart data={dailyChart}>
                                    <CartesianGrid strokeDasharray="3 3" stroke="#FDE68A" />
                                    <XAxis dataKey="name" tick={{ fontSize: 12, fill: '#92400E' }} />
                                    <YAxis tick={{ fontSize: 11, fill: '#92400E' }} tickFormatter={v => v >= 1000000 ? `${(v / 1000000).toFixed(1)}M` : `${(v / 1000).toFixed(0)}K`} />
                                    <Tooltip formatter={(v) => [`${v.toLocaleString('vi-VN')}đ`, 'Doanh thu']} />
                                    <Bar dataKey="revenue" fill="#F59E0B" radius={[4, 4, 0, 0]} />
                                </BarChart>
                            </ResponsiveContainer>
                        </CardContent>
                    </Card>
                    <Card className="border-amber-200">
                        <CardHeader className="pb-2"><CardTitle className="text-base text-amber-900">Món bán chạy</CardTitle></CardHeader>
                        <CardContent>
                            <div className="space-y-2">
                                {topItems.map((item, i) => (
                                    <div key={i} className="flex items-center justify-between">
                                        <span className="text-sm text-amber-800">{i + 1}. {item.name}</span>
                                        <Badge className="bg-amber-100 text-amber-700">{item.qty} phần</Badge>
                                    </div>
                                ))}
                                {topItems.length === 0 && <p className="text-gray-400 text-sm">Chưa có dữ liệu</p>}
                            </div>
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="weekly" className="space-y-4 mt-4">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <CompareCard label="Doanh thu tuần này" current={sum(periodData.thisWeekPayments)} previous={sum(periodData.lastWeekPayments)} />
                        <CompareCard label="Hóa đơn tuần này" current={periodData.thisWeekPayments.length} previous={periodData.lastWeekPayments.length} />
                    </div>
                </TabsContent>

                <TabsContent value="monthly" className="space-y-4 mt-4">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <CompareCard label="Doanh thu tháng này" current={sum(periodData.thisMonthPayments)} previous={sum(periodData.lastMonthPayments)} />
                        <CompareCard label="Hóa đơn tháng này" current={periodData.thisMonthPayments.length} previous={periodData.lastMonthPayments.length} />
                    </div>
                </TabsContent>

                <TabsContent value="shifts" className="mt-4">
                    <div className="space-y-2">
                        {visibleShifts.length === 0 && <p className="text-amber-600 text-center py-8">Chưa có ca làm việc nào</p>}
                        {visibleShifts.map(shift => {
                            const user = users.find(u => u.id === shift.userId);
                            return (
                                <div key={shift.id} className="border border-amber-200 rounded-xl px-4 py-3 bg-white">
                                    <div className="flex items-center justify-between">
                                        <div>
                                            <p className="font-medium text-amber-900">{user?.fullName}</p>
                                            <p className="text-xs text-gray-500">
                                                {format(new Date(shift.startTime), 'dd/MM/yyyy HH:mm')}
                                                {shift.endTime ? ` → ${format(new Date(shift.endTime), 'HH:mm')}` : ' → Đang làm việc'}
                                            </p>
                                        </div>
                                        <div className="text-right">
                                            <p className="font-bold text-amber-700">{shift.totalRevenue.toLocaleString('vi-VN')}đ</p>
                                            <p className="text-xs text-gray-500">{shift.totalBills} hóa đơn</p>
                                        </div>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </TabsContent>
            </Tabs>
        </div>
    );
}