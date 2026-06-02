import React, { useState, useMemo } from 'react';
import { useData } from '@/lib/DataContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';

export default function TableForm({ table = null, tables: propTables = null, areas: propAreas = null, onClose, onSaved }) {
    const contextData = useData();

    const tables = propTables || contextData.tables;
    const areas = propAreas || contextData.areas;
    const { addTable, updateTable } = contextData;

    // Auto suggest next table number if creating new table
    const defaultNumber = useMemo(() => {
        if (table) return table.number;
        const maxNumber = tables.reduce((max, t) => Math.max(max, t.number), 0);
        return maxNumber + 1;
    }, [table, tables]);

    const [form, setForm] = useState({
        number: table?.number || defaultNumber,
        areaId: table?.areaId || (areas[0]?.id || 1),
        capacity: table?.capacity || 4,
    });
    
    const [error, setError] = useState('');

    const handleSave = () => {
        const num = parseInt(form.number.toString());
        const cap = parseInt(form.capacity.toString());

        if (!num || num <= 0) {
            setError('Số bàn phải là số nguyên dương!');
            return;
        }

        if (!cap || cap <= 0) {
            setError('Sức chứa tối đa phải lớn hơn 0!');
            return;
        }

        // Check for duplicate number
        const duplicate = tables.find(t => t.number === num && t.id !== table?.id);
        if (duplicate) {
            setError(`Bàn B${num} đã tồn tại! Vui lòng chọn số bàn khác.`);
            return;
        }

        if (table) {
            // Edit existing table
            updateTable(table.id, {
                id: table.id,
                number: num,
                areaId: parseInt(form.areaId.toString()),
                capacity: cap
            })
            .then(() => onSaved())
            .catch(err => {
                setError(err.response?.data?.message || 'Không thể lưu bàn ăn');
            });
        } else {
            // Add new table
            addTable({
                number: num,
                areaId: parseInt(form.areaId.toString()),
                capacity: cap
            })
            .then(() => onSaved())
            .catch(err => {
                setError(err.response?.data?.message || 'Không thể tạo bàn ăn');
            });
        }
    };

    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-sm">
                <DialogHeader>
                    <DialogTitle className="text-amber-900">
                        {table ? `Sửa bàn B${table.number}` : 'Thêm bàn mới'}
                    </DialogTitle>
                </DialogHeader>
                <div className="space-y-4">
                    <div className="space-y-1">
                        <Label className="text-amber-800">Số bàn *</Label>
                        <Input 
                            type="number"
                            value={form.number} 
                            onChange={e => {
                                setError('');
                                setForm(f => ({ ...f, number: e.target.value }));
                            }} 
                            className="border-amber-200 focus:border-amber-500 mt-1" 
                            placeholder="Số bàn (ví dụ: 11)"
                        />
                    </div>
                    <div className="space-y-1">
                        <Label className="text-amber-800">Khu vực</Label>
                        <select 
                            value={form.areaId} 
                            onChange={e => setForm(f => ({ ...f, areaId: e.target.value }))} 
                            className="w-full mt-1 border border-amber-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-amber-500 bg-white"
                        >
                            {areas.map(a => (
                                <option key={a.id} value={a.id}>{a.name}</option>
                            ))}
                        </select>
                    </div>
                    <div className="space-y-1">
                        <Label className="text-amber-800">Sức chứa (số khách tối đa) *</Label>
                        <Input 
                            type="number"
                            value={form.capacity} 
                            onChange={e => setForm(f => ({ ...f, capacity: e.target.value }))} 
                            className="border-amber-200 focus:border-amber-500 mt-1" 
                            placeholder="Số lượng người ngồi"
                        />
                    </div>
                    {error && (
                        <p className="text-xs font-medium text-red-600 bg-red-50 p-2.5 rounded-lg border border-red-200">
                            ⚠️ {error}
                        </p>
                    )}
                    <Button 
                        onClick={handleSave} 
                        className="w-full bg-amber-500 hover:bg-amber-600 text-white rounded-xl h-11"
                    >
                        Lưu thông tin
                    </Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}
