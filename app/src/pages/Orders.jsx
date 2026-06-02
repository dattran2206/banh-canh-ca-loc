import React, { useEffect, useState } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { useData } from '@/lib/DataContext';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { format } from 'date-fns';
import PaymentDialog from '@/components/PaymentDialog';
import apiClient from '@/api/apiClient';
import { useToast } from '@/components/ui/use-toast';
import { ConfirmDialog } from '@/components/ui/confirm';

const statusLabel = { pending: 'Chờ', confirmed: 'Xác nhận', preparing: 'Chế biến', ready: 'Sẵn sàng', paid: 'Đã TT', cancelled: 'Đã hủy' };
const statusColor = {
    pending: 'bg-gray-100 text-gray-700',
    confirmed: 'bg-blue-100 text-blue-700',
    preparing: 'bg-amber-100 text-amber-700',
    ready: 'bg-green-100 text-green-700',
    paid: 'bg-gray-100 text-gray-400',
    cancelled: 'bg-rose-105 text-rose-700 border-rose-200 text-rose-700',
};

export default function Orders() {
    const { currentUser } = useAppAuth();
    const { orders: activeOrders, updateOrderStatus } = useData();
    const { toast } = useToast();
    const [filter, setFilter] = useState('active');
    const [payingOrderId, setPayingOrderId] = useState(null);
    const [payingTable, setPayingTable] = useState(null);
    const [historyOrders, setHistoryOrders] = useState([]);
    const [loadingHistory, setLoadingHistory] = useState(false);
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [confirmConfig, setConfirmConfig] = useState({
        title: "",
        description: "",
        onConfirm: () => {}
    });

    const handleCancelOrder = (orderId, orderNumber) => {
        setConfirmConfig({
            title: "Xác nhận hủy đơn",
            description: `Bạn có chắc chắn muốn hủy đơn hàng #${orderNumber}?`,
            onConfirm: async () => {
                try {
                    await updateOrderStatus(orderId, 'cancelled');
                    toast({
                        title: "Hủy đơn hàng thành công",
                        description: `Đơn hàng #${orderNumber} đã được hủy.`,
                    });
                    loadHistory(); // Reload history just in case we are in history tab
                } catch (err) {
                    console.error("Error cancelling order", err);
                    toast({
                        variant: "destructive",
                        title: "Lỗi",
                        description: "Không thể hủy đơn hàng.",
                    });
                }
            }
        });
        setConfirmOpen(true);
    };

    const loadHistory = () => {
        if (filter !== 'active') {
            setLoadingHistory(true);
            const status = filter === 'paid' ? 'paid' : undefined;
            apiClient.get('/orders', { params: { status } })
                .then(res => {
                    setHistoryOrders(res.data);
                })
                .catch(err => {
                    console.error("Error loading order history", err);
                })
                .finally(() => {
                    setLoadingHistory(false);
                });
        }
    };

    useEffect(() => {
        loadHistory();
    }, [filter]);

    const displayedOrders = filter === 'active' ? activeOrders : historyOrders;

    const canPay = currentUser?.role === 'cashier' || currentUser?.role === 'admin';

    return (
        <div className="p-4 md:p-6 space-y-4">
            <h1 className="text-2xl font-bold text-amber-900">Danh sách Order</h1>

            <div className="flex gap-2">
                {['active', 'paid', 'all'].map(f => (
                    <button
                        key={f}
                        onClick={() => setFilter(f)}
                        className={cn(
                            'px-4 py-1.5 rounded-full text-sm font-medium transition-colors',
                            filter === f ? 'bg-amber-500 text-white' : 'bg-amber-100 text-amber-700 hover:bg-amber-200'
                        )}
                    >
                        {f === 'active' ? 'Đang xử lý' : f === 'paid' ? 'Đã thanh toán' : 'Tất cả'}
                    </button>
                ))}
            </div>

            <div className="space-y-3">
                {loadingHistory ? (
                    <div className="text-center py-12 text-amber-700">Đang tải danh sách...</div>
                ) : (
                    <>
                        {displayedOrders.length === 0 && (
                            <p className="text-amber-600 text-center py-12">Không có order nào</p>
                        )}
                        {displayedOrders.map(order => {
                            const total = (order.items || []).reduce((s, i) => s + (i.menuItem?.price || 0) * i.quantity, 0);
                            return (
                                <div key={order.id} className="border border-amber-200 rounded-xl bg-white overflow-hidden">
                                    <div className="flex items-center justify-between px-4 py-2 bg-amber-50">
                                        <div className="flex items-center gap-2">
                                            <span className="font-bold text-amber-900">Bàn {order.table?.number} — {order.orderNumber}</span>
                                            <Badge className={cn('text-xs', statusColor[order.status])}>{statusLabel[order.status]}</Badge>
                                        </div>
                                        <span className="text-xs text-amber-600">{format(new Date(order.createdAt), 'dd/MM HH:mm')}</span>
                                    </div>
                                    <div className="px-4 py-2 space-y-1">
                                        {(order.items || []).map(item => (
                                            <div key={item.id} className="flex justify-between text-sm">
                                                <span className="text-gray-700">
                                                    {item.menuItem?.name} × {item.quantity}
                                                    {item.note && <span className="text-xs text-gray-400 ml-1">({item.note})</span>}
                                                </span>
                                                <span className="text-amber-700">{((item.menuItem?.price || 0) * item.quantity).toLocaleString('vi-VN')}đ</span>
                                            </div>
                                        ))}
                                        <div className="flex justify-between font-bold text-sm border-t border-amber-100 pt-1">
                                            <span>Tổng</span>
                                            <span className="text-amber-700">{total.toLocaleString('vi-VN')}đ</span>
                                        </div>
                                    </div>
                                    {['confirmed', 'preparing', 'ready'].includes(order.status) && (
                                        <div className="px-4 pb-3 flex gap-2">
                                            {canPay && (
                                                <Button
                                                    size="sm"
                                                    onClick={() => { setPayingOrderId(order.id); setPayingTable(order.table); }}
                                                    className={cn('flex-1 text-white text-xs rounded-xl', order.status === 'ready' ? 'bg-green-500 hover:bg-green-600' : 'bg-amber-500 hover:bg-amber-600')}
                                                >
                                                    {order.status === 'ready' ? 'Thanh toán' : 'Xuất hóa đơn'}
                                                </Button>
                                            )}
                                            <Button
                                                size="sm"
                                                variant="destructive"
                                                onClick={() => handleCancelOrder(order.id, order.orderNumber)}
                                                className="text-xs rounded-xl bg-rose-500 hover:bg-rose-600 text-white"
                                            >
                                                Hủy
                                            </Button>
                                        </div>
                                    )}
                                </div>
                            );
                        })}
                    </>
                )}
            </div>

            {payingOrderId && payingTable && (
                <PaymentDialog
                    orderId={payingOrderId}
                    table={payingTable}
                    onClose={() => { setPayingOrderId(null); setPayingTable(null); }}
                    onPaid={() => { setPayingOrderId(null); setPayingTable(null); loadHistory(); }}
                />
            )}

            <ConfirmDialog
                open={confirmOpen}
                onOpenChange={setConfirmOpen}
                title={confirmConfig.title}
                description={confirmConfig.description}
                onConfirm={confirmConfig.onConfirm}
            />
        </div>
    );
}