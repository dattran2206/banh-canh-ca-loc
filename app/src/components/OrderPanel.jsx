import React, { useState, useMemo } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { useData } from '@/lib/DataContext';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Plus, ShoppingBag } from 'lucide-react';
import { cn } from '@/lib/utils';
import NewOrderDialog from './NewOrderDialog';
import PaymentDialog from './PaymentDialog';
import { useToast } from '@/components/ui/use-toast';

const statusLabel = { pending: 'Chờ', confirmed: 'Xác nhận', preparing: 'Chế biến', ready: 'Sẵn sàng', paid: 'Đã TT' };
const statusColor = {
    pending: 'bg-gray-100 text-gray-700',
    confirmed: 'bg-blue-100 text-blue-700',
    preparing: 'bg-amber-100 text-amber-700',
    ready: 'bg-green-100 text-green-700',
    paid: 'bg-gray-100 text-gray-500',
};

export default function OrderPanel({ table, onClose }) {
    const { currentUser, currentShift } = useAppAuth();
    const { orders: allActiveOrders } = useData();
    const { toast } = useToast();
    const [showNewOrder, setShowNewOrder] = useState(false);
    const [selectedOrderId, setSelectedOrderId] = useState(null);
    const [payingOrderId, setPayingOrderId] = useState(null);

    const activeOrders = useMemo(() => {
        return allActiveOrders.filter(o => o.tableId === table.id && ['pending', 'confirmed', 'preparing', 'ready'].includes(o.status));
    }, [allActiveOrders, table.id]);

    const handleNewOrder = () => {
        if (!currentShift && currentUser?.role !== 'admin') {
            toast({
                variant: "destructive",
                title: "Chưa bắt đầu ca",
                description: "Vui lòng bắt đầu ca làm việc trước!",
            });
            return;
        }
        setShowNewOrder(true);
    };

    const handleOrderCreated = () => {
        setShowNewOrder(false);
    };

    const handlePaymentDone = () => {
        setPayingOrderId(null);
        const stillActive = allActiveOrders.filter(o => o.tableId === table.id && ['pending', 'confirmed', 'preparing', 'ready'].includes(o.status) && o.id !== payingOrderId);
        if (stillActive.length === 0) onClose();
    };

    const canPay = currentUser?.role === 'cashier' || currentUser?.role === 'admin';

    return (
        <>
            <Dialog open={true} onOpenChange={onClose}>
                <DialogContent className="w-[95%] sm:max-w-lg max-h-[90vh] overflow-y-auto">
                    <DialogHeader>
                        <DialogTitle className="text-amber-900">
                            Bàn {table.number} — {table.area === 'indoor' ? 'Trong nhà' : 'Ngoài trời'}
                        </DialogTitle>
                    </DialogHeader>

                    <div className="space-y-3">
                        {activeOrders.length === 0 ? (
                            <div className="text-center py-8 text-amber-600">
                                <ShoppingBag className="w-10 h-10 mx-auto mb-2 opacity-40" />
                                <p>Bàn chưa có order</p>
                            </div>
                        ) : (
                            activeOrders.map(order => (
                                <div key={order.id} className="border border-amber-200 rounded-xl overflow-hidden">
                                    <div className="flex items-center justify-between px-3 py-2 bg-amber-50">
                                        <div className="flex items-center gap-2">
                                            <span className="font-bold text-amber-900">#{order.orderNumber}</span>
                                            <Badge className={cn('text-xs', statusColor[order.status])}>
                                                {statusLabel[order.status]}
                                            </Badge>
                                        </div>
                                        <span className="text-xs text-amber-600">
                                            {new Date(order.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                                        </span>
                                    </div>
                                    <div className="px-3 py-2 space-y-1">
                                        {order.items.map(item => (
                                            <div key={item.id} className="flex justify-between text-sm">
                                                <span className="text-gray-700">
                                                    {item.menuItem?.name} × {item.quantity}
                                                    {item.note && <span className="text-xs text-gray-400 ml-1">({item.note})</span>}
                                                </span>
                                                <span className="text-amber-700 font-medium">
                                                    {((item.menuItem?.price || 0) * item.quantity).toLocaleString('vi-VN')}đ
                                                </span>
                                            </div>
                                        ))}
                                        <div className="flex justify-between font-bold text-sm border-t border-amber-100 pt-1 mt-1">
                                            <span>Tổng</span>
                                            <span className="text-amber-700">
                                                {order.items.reduce((s, i) => s + (i.menuItem?.price || 0) * i.quantity, 0).toLocaleString('vi-VN')}đ
                                            </span>
                                        </div>
                                    </div>
                                    <div className="px-3 py-2 flex gap-2 bg-gray-50">
                                        {(currentUser?.role === 'waiter' || currentUser?.role === 'admin') && (
                                            <Button
                                                size="sm"
                                                variant="outline"
                                                onClick={() => setSelectedOrderId(order.id)}
                                                className="flex-1 border-amber-300 text-amber-700 text-xs"
                                            >
                                                <Plus className="w-3 h-3 mr-1" /> Thêm món
                                            </Button>
                                        )}
                                        {canPay && order.status === 'ready' && (
                                            <Button
                                                size="sm"
                                                onClick={() => setPayingOrderId(order.id)}
                                                className="flex-1 bg-green-500 hover:bg-green-600 text-white text-xs"
                                            >
                                                Thanh toán
                                            </Button>
                                        )}
                                        {canPay && order.status !== 'ready' && (
                                            <Button
                                                size="sm"
                                                onClick={() => setPayingOrderId(order.id)}
                                                className="flex-1 bg-amber-500 hover:bg-amber-600 text-white text-xs"
                                            >
                                                Xuất hóa đơn
                                            </Button>
                                        )}
                                    </div>
                                </div>
                            ))
                        )}

                        {(currentUser?.role === 'waiter' || currentUser?.role === 'admin') && (
                            <Button
                                onClick={handleNewOrder}
                                className="w-full bg-amber-500 hover:bg-amber-600 text-white rounded-xl h-11"
                            >
                                <Plus className="w-4 h-4 mr-2" /> Thêm order mới
                            </Button>
                        )}
                    </div>
                </DialogContent>
            </Dialog>

            {showNewOrder && (
                <NewOrderDialog
                    table={table}
                    onClose={() => setShowNewOrder(false)}
                    onCreated={handleOrderCreated}
                />
            )}

            {selectedOrderId && (
                <NewOrderDialog
                    table={table}
                    existingOrderId={selectedOrderId}
                    onClose={() => setSelectedOrderId(null)}
                    onCreated={() => setSelectedOrderId(null)}
                />
            )}

            {payingOrderId && (
                <PaymentDialog
                    orderId={payingOrderId}
                    table={table}
                    onClose={() => setPayingOrderId(null)}
                    onPaid={handlePaymentDone}
                />
            )}
        </>
    );
}