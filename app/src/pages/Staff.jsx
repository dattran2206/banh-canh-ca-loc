import React, { useState, useEffect } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Switch } from '@/components/ui/switch';
import { Plus, Pencil } from 'lucide-react';
import { format } from 'date-fns';
import { useToast } from '@/components/ui/use-toast';
import apiClient from '@/api/apiClient';

const roleLabel = { admin: 'Chủ quán', cashier: 'Thu ngân', waiter: 'Bồi bàn', kitchen: 'Bếp' };
const roleColor = { admin: 'bg-purple-100 text-purple-700', cashier: 'bg-blue-100 text-blue-700', waiter: 'bg-amber-100 text-amber-700', kitchen: 'bg-red-100 text-red-700' };

export default function Staff() {
    const { currentUser } = useAppAuth();
    const { toast } = useToast();
    const [users, setUsers] = useState([]);
    const [logs, setLogs] = useState([]);
    const [loadingUsers, setLoadingUsers] = useState(true);
    const [loadingLogs, setLoadingLogs] = useState(true);
    const [refresh, setRefresh] = useState(0);
    const [showForm, setShowForm] = useState(false);
    const [editUser, setEditUser] = useState(null);

    useEffect(() => {
        setLoadingUsers(true);
        apiClient.get('/staff')
            .then(res => {
                setUsers(res.data);
            })
            .catch(err => {
                console.error("Error fetching staff", err);
            })
            .finally(() => {
                setLoadingUsers(false);
            });
    }, [refresh]);

    useEffect(() => {
        setLoadingLogs(true);
        apiClient.get('/reports/activity-logs')
            .then(res => {
                setLogs(res.data);
            })
            .catch(err => {
                console.error("Error fetching activity logs", err);
            })
            .finally(() => {
                setLoadingLogs(false);
            });
    }, [refresh]);

    const handleToggleActive = async (user) => {
        if (user.id === currentUser?.id) {
            toast({
                variant: "destructive",
                title: "Thao tác bất hợp lệ",
                description: "Không thể khóa tài khoản đang đăng nhập!",
            });
            return;
        }
        try {
            await apiClient.put(`/staff/${user.id}/toggle-active`);
            setRefresh(r => r + 1);
        } catch (err) {
            console.error("Error toggling user status", err);
            toast({
                variant: "destructive",
                title: "Thao tác thất bại",
                description: err.response?.data?.message || "Không thể cập nhật trạng thái tài khoản."
            });
        }
    };

    return (
        <div className="p-4 md:p-6 space-y-4">
            <div className="flex items-center justify-between">
                <h1 className="text-2xl font-bold text-amber-900">Nhân viên</h1>
                <Button onClick={() => { setEditUser(null); setShowForm(true); }} size="sm" className="bg-amber-500 hover:bg-amber-600 text-white rounded-xl">
                    <Plus className="w-4 h-4 mr-1" /> Thêm nhân viên
                </Button>
            </div>

            <Tabs defaultValue="list">
                <TabsList className="bg-amber-100">
                    <TabsTrigger value="list" className="data-[state=active]:bg-amber-500 data-[state=active]:text-white">Danh sách</TabsTrigger>
                    <TabsTrigger value="logs" className="data-[state=active]:bg-amber-500 data-[state=active]:text-white">Lịch sử hoạt động</TabsTrigger>
                </TabsList>

                <TabsContent value="list" className="mt-4">
                    {loadingUsers ? (
                        <p className="text-amber-600 text-center py-8">Đang tải...</p>
                    ) : (
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                            {users.map(user => (
                                <div key={user.id} className={`border rounded-xl p-4 bg-white ${!user.isActive ? 'opacity-50' : 'border-amber-200'}`}>
                                    <div className="flex items-start justify-between mb-2">
                                        <div>
                                            <p className="font-semibold text-amber-900">{user.fullName}</p>
                                            <p className="text-xs text-gray-500">@{user.username}</p>
                                        </div>
                                        <Badge className={`text-xs ${roleColor[user.role]}`}>{roleLabel[user.role]}</Badge>
                                    </div>
                                    <div className="flex items-center justify-between mt-3">
                                        <div className="flex items-center gap-2">
                                            <Switch checked={user.isActive} onCheckedChange={() => handleToggleActive(user)} />
                                            <span className="text-xs text-gray-500">{user.isActive ? 'Đang hoạt động' : 'Đã khóa'}</span>
                                        </div>
                                        <Button size="sm" variant="outline" onClick={() => { setEditUser(user); setShowForm(true); }} className="border-amber-200 text-amber-700 text-xs">
                                            <Pencil className="w-3 h-3 mr-1" /> Sửa
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </TabsContent>

                <TabsContent value="logs" className="mt-4">
                    {loadingLogs ? (
                        <p className="text-amber-600 text-center py-8">Đang tải...</p>
                    ) : (
                        <div className="space-y-2">
                            {logs.length === 0 && <p className="text-amber-600 text-center py-8">Chưa có lịch sử hoạt động</p>}
                            {logs.map(log => (
                                <div key={log.id} className="border border-amber-100 rounded-xl px-4 py-2.5 bg-white flex items-center justify-between">
                                    <div>
                                        <p className="text-sm font-medium text-amber-900">{log.userFullName}</p>
                                        <p className="text-xs text-gray-600">{log.detail}</p>
                                    </div>
                                    <p className="text-xs text-gray-400">{format(new Date(log.timestamp), 'dd/MM HH:mm')}</p>
                                </div>
                            ))}
                        </div>
                    )}
                </TabsContent>
            </Tabs>

            {showForm && (
                <StaffForm
                    user={editUser}
                    onClose={() => { setShowForm(false); setEditUser(null); }}
                    onSaved={() => { setShowForm(false); setEditUser(null); setRefresh(r => r + 1); }}
                />
            )}
        </div>
    );
}

function StaffForm({ user, onSaved, onClose }) {
    const { toast } = useToast();
    const [form, setForm] = useState({
        fullName: user?.fullName || '',
        username: user?.username || '',
        password: '',
        role: user?.role || 'waiter',
    });
    const [loading, setLoading] = useState(false);

    const handleSave = async () => {
        if (!form.fullName || !form.username) {
            toast({
                variant: "destructive",
                title: "Thiếu thông tin",
                description: "Vui lòng điền đầy đủ thông tin!",
            });
            return;
        }
        setLoading(true);
        try {
            if (user) {
                await apiClient.put(`/staff/${user.id}`, {
                    fullName: form.fullName,
                    username: form.username,
                    role: form.role,
                    password: form.password || null
                });
                toast({
                    title: "Cập nhật thành công",
                    description: `Đã cập nhật thông tin nhân viên ${form.fullName}.`,
                });
            } else {
                if (!form.password) {
                    toast({
                        variant: "destructive",
                        title: "Thiếu thông tin",
                        description: "Vui lòng nhập mật khẩu!",
                    });
                    return;
                }
                await apiClient.post('/staff', {
                    fullName: form.fullName,
                    username: form.username,
                    role: form.role,
                    password: form.password
                });
                toast({
                    title: "Thêm thành công",
                    description: `Đã tạo nhân viên ${form.fullName}.`,
                });
            }
            onSaved();
        } catch (err) {
            console.error("Error saving staff user", err);
            toast({
                variant: "destructive",
                title: "Lưu thất bại",
                description: err.response?.data?.message || "Lỗi xảy ra khi lưu nhân viên."
            });
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-sm">
                <DialogHeader><DialogTitle className="text-amber-900">{user ? 'Sửa nhân viên' : 'Thêm nhân viên'}</DialogTitle></DialogHeader>
                <div className="space-y-3">
                    <div><Label className="text-amber-800">Họ tên *</Label><Input value={form.fullName} onChange={e => setForm(f => ({ ...f, fullName: e.target.value }))} className="border-amber-200 mt-1" /></div>
                    <div><Label className="text-amber-800">Tên đăng nhập *</Label><Input value={form.username} onChange={e => setForm(f => ({ ...f, username: e.target.value }))} className="border-amber-200 mt-1" /></div>
                    <div><Label className="text-amber-800">{user ? 'Mật khẩu mới (bỏ trống nếu không đổi)' : 'Mật khẩu *'}</Label><Input type="password" value={form.password} onChange={e => setForm(f => ({ ...f, password: e.target.value }))} className="border-amber-200 mt-1" /></div>
                    <div><Label className="text-amber-800">Vai trò</Label>
                        <select value={form.role} onChange={e => setForm(f => ({ ...f, role: e.target.value }))} className="w-full mt-1 border border-amber-200 rounded-lg px-3 py-2 text-sm">
                            <option value="waiter">Bồi bàn</option>
                            <option value="cashier">Thu ngân</option>
                            <option value="kitchen">Bếp</option>
                            <option value="admin">Chủ quán</option>
                        </select>
                    </div>
                    <Button onClick={handleSave} disabled={loading} className="w-full bg-amber-500 hover:bg-amber-600 text-white rounded-xl">
                        {loading ? 'Đang lưu...' : 'Lưu'}
                    </Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}