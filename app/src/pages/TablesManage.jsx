import React, { useState, useMemo } from 'react';
import { useData } from '@/lib/DataContext';
import { useAppAuth } from '@/lib/appAuth';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
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
import { useToast } from '@/components/ui/use-toast';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { Plus, Pencil, Trash2, MapPin, Users } from 'lucide-react';
import TableForm from '@/components/TableForm';
import apiClient from '@/api/apiClient';

export default function TablesManage() {
    const { currentUser } = useAppAuth();
    const { toast } = useToast();
    const { tables, areas, orders, deleteTable, deleteArea } = useData();
    const [showForm, setShowForm] = useState(false);
    const [editTable, setEditTable] = useState(null);
    const [deleteConfirmTable, setDeleteConfirmTable] = useState(null);

    // Area states
    const [showAreaForm, setShowAreaForm] = useState(false);
    const [editArea, setEditArea] = useState(null);
    const [deleteConfirmArea, setDeleteConfirmArea] = useState(null);

    const sortedTables = useMemo(() => {
        return [...tables].sort((a, b) => a.number - b.number);
    }, [tables]);

    const activeTableIds = useMemo(() => {
        return new Set(
            orders
                .filter(o => ['pending', 'confirmed', 'preparing', 'ready'].includes(o.status))
                .map(o => o.tableId)
        );
    }, [orders]);

    const handleDelete = (table) => {
        if (activeTableIds.has(table.id)) {
            toast({
                variant: "destructive",
                title: "Không thể xóa bàn!",
                description: `Bàn B${table.number} đang có khách hoặc có order đang xử lý!`,
            });
            return;
        }
        setDeleteConfirmTable(table);
    };

    const confirmDelete = async () => {
        if (!deleteConfirmTable) return;
        try {
            await deleteTable(deleteConfirmTable.id);
            toast({
                title: "Xóa bàn thành công",
                description: `Bàn B${deleteConfirmTable.number} đã được gỡ khỏi hệ thống.`,
            });
            setDeleteConfirmTable(null);
        } catch (err) {
            console.error("Error deleting table", err);
            toast({
                variant: "destructive",
                title: "Lỗi xóa bàn",
                description: err.response?.data?.message || "Không thể xóa bàn ăn này."
            });
        }
    };

    const handleDeleteArea = (area) => {
        const tablesInArea = tables.filter(t => t.areaId === area.id);
        if (tablesInArea.length > 0) {
            toast({
                variant: "destructive",
                title: "Không thể xóa khu vực!",
                description: `Khu vực "${area.name}" đang chứa ${tablesInArea.length} bàn ăn! Vui lòng chuyển hoặc xóa bàn ăn trước.`,
            });
            return;
        }
        setDeleteConfirmArea(area);
    };

    const confirmDeleteArea = async () => {
        if (!deleteConfirmArea) return;
        try {
            await deleteArea(deleteConfirmArea.id);
            toast({
                title: "Xóa khu vực thành công",
                description: `Khu vực "${deleteConfirmArea.name}" đã được gỡ khỏi hệ thống.`,
            });
            setDeleteConfirmArea(null);
        } catch (err) {
            console.error("Error deleting area", err);
            toast({
                variant: "destructive",
                title: "Không thể xóa khu vực!",
                description: err.response?.data?.message || "Có lỗi xảy ra khi xóa khu vực."
            });
        }
    };

    return (
        <div className="p-4 md:p-6 space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-amber-900">Quản lý bàn & Khu vực</h1>
                    <p className="text-amber-600 text-xs">Cấu hình danh mục khu vực và danh sách bàn ăn tương ứng</p>
                </div>
            </div>

            <Tabs defaultValue="tables" className="w-full space-y-6">
                <TabsList className="bg-amber-100 p-1 rounded-xl">
                    <TabsTrigger 
                        value="tables" 
                        className="data-[state=active]:bg-amber-500 data-[state=active]:text-white rounded-lg px-4 py-2"
                    >
                        Quản lý Bàn ăn
                    </TabsTrigger>
                    <TabsTrigger 
                        value="areas" 
                        className="data-[state=active]:bg-amber-500 data-[state=active]:text-white rounded-lg px-4 py-2"
                    >
                        Quản lý Khu vực
                    </TabsTrigger>
                </TabsList>

                <TabsContent value="tables" className="space-y-4">
                    <div className="flex justify-between items-center">
                        <h2 className="text-lg font-semibold text-amber-900">Danh sách bàn ăn</h2>
                        <Button 
                            onClick={() => { setEditTable(null); setShowForm(true); }} 
                            className="bg-amber-500 hover:bg-amber-600 text-white rounded-xl"
                        >
                            <Plus className="w-4 h-4 mr-1" /> Thêm bàn mới
                        </Button>
                    </div>

                    <div className="border border-amber-200 rounded-xl overflow-x-auto bg-white shadow-sm">
                        <table className="w-full text-sm text-left border-collapse min-w-[600px] md:min-w-0">
                            <thead className="bg-amber-50 border-b border-amber-200">
                                <tr>
                                    <th className="px-6 py-3 font-semibold text-amber-800">Số bàn</th>
                                    <th className="px-6 py-3 font-semibold text-amber-800">Khu vực</th>
                                    <th className="px-6 py-3 font-semibold text-amber-800">Sức chứa tối đa</th>
                                    <th className="px-6 py-3 font-semibold text-amber-800">Trạng thái</th>
                                    <th className="px-6 py-3 font-semibold text-amber-800 text-right">Thao tác</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-amber-100">
                                {sortedTables.map(table => {
                                    const isActive = activeTableIds.has(table.id);
                                    const areaName = table.area?.name || "Khu vực khác";
                                    return (
                                        <tr key={table.id} className="hover:bg-amber-50/50 transition-colors">
                                            <td className="px-6 py-4 font-bold text-amber-950 text-base">Bàn B{table.number}</td>
                                            <td className="px-6 py-4 text-gray-700 flex items-center gap-1.5">
                                                <MapPin className="w-4 h-4 text-amber-600" />
                                                {areaName}
                                            </td>
                                            <td className="px-6 py-4 text-gray-700">
                                                <span className="flex items-center gap-1">
                                                    <Users className="w-4 h-4 text-gray-400" /> {table.capacity} khách
                                                </span>
                                            </td>
                                            <td className="px-6 py-4">
                                                {isActive ? (
                                                    <Badge className="bg-amber-100 text-amber-800 border-amber-300">Có khách</Badge>
                                                ) : (
                                                    <Badge className="bg-green-50 text-green-700 border-green-200">Trống</Badge>
                                                )}
                                            </td>
                                            <td className="px-6 py-4 text-right">
                                                <div className="flex items-center justify-end gap-2">
                                                    <Button 
                                                        onClick={() => { setEditTable(table); setShowForm(true); }}
                                                        variant="outline" 
                                                        size="sm" 
                                                        className="border-amber-200 text-amber-700 hover:bg-amber-50 text-xs h-8"
                                                    >
                                                        <Pencil className="w-3.5 h-3.5 mr-1" /> Sửa
                                                    </Button>
                                                    <Button 
                                                        onClick={() => handleDelete(table)}
                                                        variant="outline" 
                                                        size="sm" 
                                                        className="border-red-200 text-red-500 hover:bg-red-50 text-xs h-8"
                                                    >
                                                        <Trash2 className="w-3.5 h-3.5 mr-1" /> Xóa
                                                    </Button>
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })}
                                {sortedTables.length === 0 && (
                                    <tr>
                                        <td colSpan={5} className="text-center py-10 text-gray-400">
                                            Chưa có bàn ăn nào được cấu hình
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                </TabsContent>

                <TabsContent value="areas" className="space-y-4">
                    <div className="flex justify-between items-center">
                        <h2 className="text-lg font-semibold text-amber-900">Danh mục khu vực</h2>
                        <Button 
                            onClick={() => { setEditArea(null); setShowAreaForm(true); }} 
                            className="bg-amber-500 hover:bg-amber-600 text-white rounded-xl"
                        >
                            <Plus className="w-4 h-4 mr-1" /> Thêm khu vực mới
                        </Button>
                    </div>

                    <div className="border border-amber-200 rounded-xl overflow-x-auto bg-white shadow-sm">
                        <table className="w-full text-sm text-left border-collapse min-w-[500px] md:min-w-0">
                            <thead className="bg-amber-50 border-b border-amber-200">
                                <tr>
                                    <th className="px-6 py-3 font-semibold text-amber-800">Khu vực</th>
                                    <th className="px-6 py-3 font-semibold text-amber-800">Số lượng bàn</th>
                                    <th className="px-6 py-3 font-semibold text-amber-800 text-right">Thao tác</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-amber-100">
                                {areas.map(area => {
                                    const tablesInArea = tables.filter(t => t.areaId === area.id);
                                    return (
                                        <tr key={area.id} className="hover:bg-amber-50/50 transition-colors">
                                            <td className="px-6 py-4 font-bold text-amber-950 text-base flex items-center gap-2">
                                                <MapPin className="w-4 h-4 text-amber-600" />
                                                {area.name}
                                            </td>
                                            <td className="px-6 py-4 text-gray-700">
                                                {tablesInArea.length} bàn
                                            </td>
                                            <td className="px-6 py-4 text-right">
                                                <div className="flex items-center justify-end gap-2">
                                                    <Button 
                                                        onClick={() => { setEditArea(area); setShowAreaForm(true); }}
                                                        variant="outline" 
                                                        size="sm" 
                                                        className="border-amber-200 text-amber-700 hover:bg-amber-50 text-xs h-8"
                                                    >
                                                        <Pencil className="w-3.5 h-3.5 mr-1" /> Sửa
                                                    </Button>
                                                    <Button 
                                                        onClick={() => handleDeleteArea(area)}
                                                        variant="outline" 
                                                        size="sm" 
                                                        className="border-red-200 text-red-500 hover:bg-red-50 text-xs h-8"
                                                    >
                                                        <Trash2 className="w-3.5 h-3.5 mr-1" /> Xóa
                                                    </Button>
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })}
                                {areas.length === 0 && (
                                    <tr>
                                        <td colSpan={3} className="text-center py-10 text-gray-400">
                                            Chưa có khu vực nào được cấu hình
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                </TabsContent>
            </Tabs>

            {showForm && (
                <TableForm
                    table={editTable}
                    tables={tables}
                    areas={areas}
                    onClose={() => { setShowForm(false); setEditTable(null); }}
                    onSaved={() => { setShowForm(false); setEditTable(null); }}
                />
            )}

            {showAreaForm && (
                <AreaForm
                    area={editArea}
                    areas={areas}
                    onClose={() => { setShowAreaForm(false); setEditArea(null); }}
                    onSaved={() => { setShowAreaForm(false); setEditArea(null); }}
                />
            )}

            {deleteConfirmTable && (
                <AlertDialog open={true} onOpenChange={(open) => !open && setDeleteConfirmTable(null)}>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle className="text-amber-900">Xác nhận xóa bàn</AlertDialogTitle>
                            <AlertDialogDescription>
                                Bạn có chắc chắn muốn xóa bàn B{deleteConfirmTable.number}? Hành động này sẽ gỡ bỏ bàn này vĩnh viễn khỏi sơ đồ bàn.
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                            <AlertDialogCancel className="border-amber-200 text-amber-800">Hủy</AlertDialogCancel>
                            <AlertDialogAction 
                                onClick={confirmDelete}
                                className="bg-red-500 hover:bg-red-600 text-white"
                            >
                                Xác nhận xóa
                            </AlertDialogAction>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            )}

            {deleteConfirmArea && (
                <AlertDialog open={true} onOpenChange={(open) => !open && setDeleteConfirmArea(null)}>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle className="text-amber-900">Xác nhận xóa khu vực</AlertDialogTitle>
                            <AlertDialogDescription>
                                Bạn có chắc chắn muốn xóa khu vực "{deleteConfirmArea.name}"? Hành động này không thể hoàn tác.
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                            <AlertDialogCancel className="border-amber-200 text-amber-800">Hủy</AlertDialogCancel>
                            <AlertDialogAction 
                                onClick={confirmDeleteArea}
                                className="bg-red-500 hover:bg-red-600 text-white"
                            >
                                Xác nhận xóa
                            </AlertDialogAction>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            )}
        </div>
    );
}

function AreaForm({ area, areas, onClose, onSaved }) {
    const { addArea, refreshAreas } = useData();
    const [name, setName] = useState(area?.name || '');
    const [error, setError] = useState('');

    const handleSave = async () => {
        const trimmedName = name.trim();
        if (!trimmedName) {
            setError('Tên khu vực không được để trống!');
            return;
        }

        // Check duplicate area name
        const duplicate = areas.find(a => a.name.toLowerCase() === trimmedName.toLowerCase() && a.id !== area?.id);
        if (duplicate) {
            setError(`Khu vực "${trimmedName}" đã tồn tại!`);
            return;
        }

        try {
            if (area) {
                // Edit existing area
                await apiClient.put(`/tables/areas/${area.id}`, { id: area.id, name: trimmedName });
                await refreshAreas();
            } else {
                // Add new area
                await addArea({ name: trimmedName });
            }
            onSaved();
        } catch (err) {
            console.error("Error saving area", err);
            setError("Có lỗi xảy ra khi lưu khu vực.");
        }
    };

    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-sm">
                <DialogHeader>
                    <DialogTitle className="text-amber-900">
                        {area ? `Sửa khu vực` : 'Thêm khu vực mới'}
                    </DialogTitle>
                </DialogHeader>
                <div className="space-y-4">
                    <div className="space-y-1">
                        <Label className="text-amber-800">Tên khu vực *</Label>
                        <Input 
                            type="text"
                            value={name} 
                            onChange={e => {
                                setError('');
                                setName(e.target.value);
                            }} 
                            className="border-amber-200 focus:border-amber-500 mt-1" 
                            placeholder="Ví dụ: Tầng 2, Sân thượng,..."
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
