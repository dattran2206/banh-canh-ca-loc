import React, { useState, useMemo, useEffect } from 'react';
import { getList, setList, KEYS } from '@/lib/storage';
import { useAppAuth } from '@/lib/appAuth';
import { doLogActivity } from '@/lib/appAuth';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { ChefHat, Clock } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useToast } from '@/components/ui/use-toast';

function autoDeductInventory(orderId) {
    const orderItems = getList(KEYS.ORDER_ITEMS).filter(i => i.orderId === orderId);
    const recipeItems = getList(KEYS.RECIPE_ITEMS);
    const ingredients = getList(KEYS.INGREDIENTS);
    const warnings = [];

    orderItems.forEach(oi => {
        const recipes = recipeItems.filter(r => r.menuItemId === oi.menuItemId);
        recipes.forEach(recipe => {
            const deduct = (recipe.quantity / recipe.yieldPercent) * oi.quantity;
            const ingIdx = ingredients.findIndex(i => i.id === recipe.ingredientId);
            if (ingIdx !== -1) {
                ingredients[ingIdx] = {
                    ...ingredients[ingIdx],
                    currentStock: Math.max(0, ingredients[ingIdx].currentStock - deduct)
                };
                if (ingredients[ingIdx].currentStock <= ingredients[ingIdx].minThreshold) {
                    warnings.push(ingredients[ingIdx].name);
                }
            }
        });
    });

    setList(KEYS.INGREDIENTS, ingredients);
    return warnings;
}

export default function Kitchen() {
    const { currentUser } = useAppAuth();
    const { toast } = useToast();
    const [refresh, setRefresh] = useState(0);

    const orders = useMemo(() => {
        const allOrders = getList(KEYS.ORDERS);
        const orderItems = getList(KEYS.ORDER_ITEMS);
        const menuItems = getList(KEYS.MENU_ITEMS);
        const tables = getList(KEYS.TABLES);

        return allOrders
            .filter(o => ['confirmed', 'preparing'].includes(o.status))
            .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())
            .map(o => ({
                ...o,
                table: tables.find(t => t.id === o.tableId),
                items: orderItems
                    .filter(i => i.orderId === o.id)
                    .map(i => ({ ...i, menuItem: menuItems.find(m => m.id === i.menuItemId) })),
            }));
    }, [refresh]);

    useEffect(() => {
        const interval = setInterval(() => setRefresh(r => r + 1), 10000);
        return () => clearInterval(interval);
    }, []);

    const updateOrderStatus = (orderId, newStatus) => {
        const orders = getList(KEYS.ORDERS);
        const idx = orders.findIndex(o => o.id === orderId);
        if (idx !== -1) {
            orders[idx] = { ...orders[idx], status: newStatus };
            setList(KEYS.ORDERS, orders);
            if (newStatus === 'ready') {
                const warnings = autoDeductInventory(orderId);
                if (warnings.length > 0) {
                    toast({
                        variant: "destructive",
                        title: "Cảnh báo tồn kho thấp",
                        description: warnings.join(', '),
                    });
                }
                doLogActivity(currentUser?.id, 'order_ready', `Order ${orders[idx].orderNumber} bàn ${orders[idx].tableId} sẵn sàng`);
            } else {
                doLogActivity(currentUser?.id, 'order_preparing', `Bắt đầu chế biến order ${orders[idx].orderNumber}`);
            }
            setRefresh(r => r + 1);
        }
    };

    const elapsed = (createdAt) => {
        const mins = Math.floor((Date.now() - new Date(createdAt).getTime()) / 60000);
        return `${mins} phút`;
    };

    return (
        <div className="min-h-screen bg-gray-900 p-4">
            <div className="flex items-center gap-3 mb-6">
                <ChefHat className="w-8 h-8 text-amber-400" />
                <h1 className="text-2xl font-bold text-white">Bếp — Danh sách order</h1>
                <Badge className="bg-amber-500 text-white">{orders.length} order</Badge>
            </div>

            {orders.length === 0 ? (
                <div className="text-center py-20 text-gray-500">
                    <ChefHat className="w-16 h-16 mx-auto mb-4 opacity-30" />
                    <p className="text-xl">Không có order nào cần chế biến</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {orders.map(order => (
                        <div
                            key={order.id}
                            className={cn(
                                'rounded-2xl border-2 p-4 flex flex-col gap-3',
                                order.status === 'confirmed' ? 'bg-gray-800 border-blue-500' : 'bg-gray-800 border-amber-500'
                            )}
                        >
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="text-white text-xl font-bold">
                                        Bàn {order.table?.number} — {order.orderNumber}
                                    </p>
                                    <div className="flex items-center gap-2 mt-1">
                                        <Badge className={cn(
                                            'text-sm px-3 py-1',
                                            order.status === 'confirmed' ? 'bg-blue-600 text-white' : 'bg-amber-500 text-white'
                                        )}>
                                            {order.status === 'confirmed' ? 'Chờ chế biến' : 'Đang chế biến'}
                                        </Badge>
                                        <span className="text-gray-400 text-sm flex items-center gap-1">
                                            <Clock className="w-3 h-3" /> {elapsed(order.createdAt)}
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <div className="space-y-2">
                                {order.items.map(item => (
                                    <div key={item.id} className="bg-gray-700 rounded-xl px-3 py-2">
                                        <div className="flex justify-between items-start">
                                            <span className="text-white text-lg font-semibold">{item.menuItem?.name}</span>
                                            <span className="text-amber-400 text-xl font-bold ml-2">×{item.quantity}</span>
                                        </div>
                                        {item.note && (
                                            <p className="text-yellow-400 text-sm mt-0.5">📝 {item.note}</p>
                                        )}
                                    </div>
                                ))}
                            </div>

                            <div className="flex gap-2 mt-auto">
                                {order.status === 'confirmed' && (
                                    <Button
                                        onClick={() => updateOrderStatus(order.id, 'preparing')}
                                        className="flex-1 bg-amber-500 hover:bg-amber-600 text-white rounded-xl h-12 text-base font-bold"
                                    >
                                        Bắt đầu chế biến
                                    </Button>
                                )}
                                {order.status === 'preparing' && (
                                    <Button
                                        onClick={() => updateOrderStatus(order.id, 'ready')}
                                        className="flex-1 bg-green-500 hover:bg-green-600 text-white rounded-xl h-12 text-base font-bold"
                                    >
                                        ✓ Xong — Sẵn sàng
                                    </Button>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}