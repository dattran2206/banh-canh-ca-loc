import React, { useState, useEffect } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import { TrendingUp, TrendingDown, Minus } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import { format } from 'date-fns';
import apiClient from '@/api/apiClient';

function CompareCard({ label, current, previous }) {
    const diff = current - previous;
    const pct = previous > 0 ? Math.round((diff / previous) * 100) : (current > 0 ? 100 : 0);
    const isUp = diff > 0;
    const isDown = diff < 0;
    return (
        <Card className="border-amber-200">
            <CardContent className="pt-4 pb-4">
                <p className="text-xs text-amber-600 mb-1">{label}</p>
                <p className="text-2xl font-bold text-amber-900">
                    {typeof current === 'number' && current > 999 ? `${current.toLocaleString('vi-VN')}đ` : current}
                </p>
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
    const { currentUser } = useAppAuth();
    const [summary, setSummary] = useState(null);
    const [shifts, setShifts] = useState([]);
    const [loadingSummary, setLoadingSummary] = useState(true);
    const [loadingShifts, setLoadingShifts] = useState(true);

    useEffect(() => {
        if (currentUser?.role === 'admin') {
            apiClient.get('/reports/summary')
                .then(res => {
                    setSummary(res.data);
                })
                .catch(err => {
                    console.error("Error loading report summary", err);
                })
                .finally(() => {
                    setLoadingSummary(false);
                });
        } else {
            setLoadingSummary(false);
        }

        apiClient.get('/auth/shifts')
            .then(res => {
                setShifts(res.data);
            })
            .catch(err => {
                console.error("Error loading shifts", err);
            })
            .finally(() => {
                setLoadingShifts(false);
            });
    }, [currentUser]);

    if (currentUser?.role !== 'admin') {
        return (
            <div className="p-4 md:p-6 space-y-6">
                <h1 className="text-2xl font-bold text-amber-900">Ca làm việc của tôi</h1>
                <div className="space-y-2">
                    {loadingShifts ? (
                        <p className="text-amber-600 text-center py-8">Đang tải...</p>
                    ) : (
                        <>
                            {shifts.length === 0 && <p className="text-amber-600 text-center py-8">Chưa có ca làm việc nào</p>}
                            {shifts.map(shift => (
                                <div key={shift.id} className="border border-amber-200 rounded-xl px-4 py-3 bg-white">
                                    <div className="flex items-center justify-between">
                                        <div>
                                            <p className="font-medium text-amber-900">{shift.user?.fullName || currentUser?.fullName}</p>
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
                            ))}
                        </>
                    )}
                </div>
            </div>
        );
    }

    if (loadingSummary || !summary) {
        return (
            <div className="fixed inset-0 flex items-center justify-center bg-amber-50">
                <div className="text-center">
                    <div className="w-10 h-10 border-4 border-amber-200 border-t-amber-500 rounded-full animate-spin mx-auto mb-3"></div>
                    <p className="text-amber-700 text-sm">Đang tải báo cáo...</p>
                </div>
            </div>
        );
    }

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
                        <CompareCard label="Doanh thu hôm nay" current={summary.todayRevenue} previous={summary.yesterdayRevenue} />
                        <CompareCard label="Số hóa đơn hôm nay" current={summary.todayBills} previous={summary.yesterdayBills} />
                    </div>
                    <Card className="border-amber-200">
                        <CardHeader className="pb-2"><CardTitle className="text-base text-amber-900">Doanh thu 7 ngày qua</CardTitle></CardHeader>
                        <CardContent>
                            <ResponsiveContainer width="100%" height={200}>
                                <BarChart data={summary.dailyChart}>
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
                                {(summary.topItems || []).map((item, i) => (
                                    <div key={i} className="flex items-center justify-between">
                                        <span className="text-sm text-amber-800">{i + 1}. {item.name}</span>
                                        <Badge className="bg-amber-100 text-amber-700">{item.qty} phần</Badge>
                                    </div>
                                ))}
                                {(!summary.topItems || summary.topItems.length === 0) && <p className="text-gray-400 text-sm">Chưa có dữ liệu</p>}
                            </div>
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="weekly" className="space-y-4 mt-4">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <CompareCard label="Doanh thu tuần này" current={summary.thisWeekRevenue} previous={summary.lastWeekRevenue} />
                        <CompareCard label="Hóa đơn tuần này" current={summary.thisWeekBills} previous={summary.lastWeekBills} />
                    </div>
                </TabsContent>

                <TabsContent value="monthly" className="space-y-4 mt-4">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <CompareCard label="Doanh thu tháng này" current={summary.thisMonthRevenue} previous={summary.lastMonthRevenue} />
                        <CompareCard label="Hóa đơn tháng này" current={summary.thisMonthBills} previous={summary.lastMonthBills} />
                    </div>
                </TabsContent>

                <TabsContent value="shifts" className="mt-4">
                    <div className="space-y-2">
                        {loadingShifts ? (
                            <p className="text-amber-600 text-center py-8">Đang tải...</p>
                        ) : (
                            <>
                                {shifts.length === 0 && <p className="text-amber-600 text-center py-8">Chưa có ca làm việc nào</p>}
                                {shifts.map(shift => (
                                    <div key={shift.id} className="border border-amber-200 rounded-xl px-4 py-3 bg-white">
                                        <div className="flex items-center justify-between">
                                            <div>
                                                <p className="font-medium text-amber-900">{shift.user?.fullName}</p>
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
                                ))}
                            </>
                        )}
                    </div>
                </TabsContent>
            </Tabs>
        </div>
    );
}