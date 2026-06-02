import React, { useState, useMemo } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { useData } from '@/lib/DataContext';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Users, Plus } from 'lucide-react';
import { cn } from '@/lib/utils';
import OrderPanel from '@/components/OrderPanel';
import TableForm from '@/components/TableForm';

function getTableStatus(tableId, orders) {
    const active = orders.filter(o => o.tableId === tableId && ['pending', 'confirmed', 'preparing', 'ready'].includes(o.status));
    if (active.length === 0) return 'empty';
    return 'occupied';
}

export default function Tables() {
    const { currentUser } = useAppAuth();
    const { tables, orders, areas } = useData();
    const [selectedTable, setSelectedTable] = useState(null);
    const [showAddForm, setShowAddForm] = useState(false);

    const groupedByArea = useMemo(() => {
        const finalAreas = areas.length > 0 ? areas : [
            { id: 1, name: 'Trong nhà' },
            { id: 2, name: 'Ngoài trời' }
        ];

        return tables.reduce((acc, t) => {
            const areaObj = finalAreas.find(a => a.id === t.areaId);
            const areaName = areaObj ? areaObj.name : 'Khác';
            if (!acc[areaName]) acc[areaName] = [];
            acc[areaName].push({ ...t, status: getTableStatus(t.id, orders) });
            return acc;
        }, {});
    }, [tables, orders, areas]);

    const handleTableClick = (table) => {
        setSelectedTable(table);
    };

    const statusColor = {
        empty: 'bg-green-50 border-green-300 text-green-800',
        occupied: 'bg-amber-50 border-amber-400 text-amber-900',
        billing: 'bg-purple-50 border-purple-300 text-purple-900',
    };

    const statusLabel = { empty: 'Trống', occupied: 'Có khách', billing: 'Thanh toán' };

    return (
        <div className="p-4 md:p-6">
            <div className="flex items-center justify-between mb-6">
                <h1 className="text-2xl font-bold text-amber-900">Sơ đồ bàn</h1>
                {currentUser?.role === 'admin' && (
                    <Button onClick={() => setShowAddForm(true)} size="sm" className="bg-amber-500 hover:bg-amber-600 text-white rounded-xl">
                        <Plus className="w-4 h-4 mr-1" /> Thêm nhanh bàn
                    </Button>
                )}
            </div>

            <div className="space-y-6">
                {Object.entries(groupedByArea).map(([area, areaTables]) => (
                    <div key={area}>
                        <h2 className="text-sm font-semibold text-amber-700 uppercase tracking-wide mb-3">{area}</h2>
                        <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 lg:grid-cols-6 gap-3">
                            {areaTables.map(table => {
                                const activeOrders = orders.filter(o => o.tableId === table.id && ['pending', 'confirmed', 'preparing', 'ready'].includes(o.status));
                                return (
                                    <button
                                        key={table.id}
                                        onClick={() => handleTableClick(table)}
                                        className={cn(
                                            'relative border-2 rounded-xl p-3 text-center transition-all hover:scale-105 active:scale-95 min-h-[80px] flex flex-col items-center justify-center gap-1',
                                            statusColor[table.status]
                                        )}
                                    >
                                        <span className="text-xl font-bold">B{table.number}</span>
                                        <span className="text-xs">{statusLabel[table.status]}</span>
                                        {activeOrders.length > 0 && (
                                            <Badge className="absolute -top-2 -right-2 bg-amber-500 text-white text-xs px-1.5 py-0.5 rounded-full">
                                                {activeOrders.length}
                                            </Badge>
                                        )}
                                        <div className="flex items-center gap-1 text-xs opacity-60">
                                            <Users className="w-3 h-3" /> {table.capacity}
                                        </div>
                                    </button>
                                );
                            })}
                        </div>
                    </div>
                ))}
            </div>

            {selectedTable && (
                <OrderPanel
                    table={selectedTable}
                    onClose={() => setSelectedTable(null)}
                />
            )}

            {showAddForm && (
                <TableForm
                    onClose={() => setShowAddForm(false)}
                    onSaved={() => setShowAddForm(false)}
                />
            )}
        </div>
    );
}