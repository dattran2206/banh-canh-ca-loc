import React, { useState } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Loader2 } from 'lucide-react';

export default function Login() {
    const { login } = useAppAuth();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);
        try {
            await login(username, password);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-amber-50 flex items-center justify-center p-4">
            <Card className="w-full max-w-sm shadow-xl border-amber-200">
                <CardHeader className="text-center pb-2">
                    <div className="flex justify-center mb-3">
                        <img src="/banh_canh_logo.png" alt="Logo Bánh Canh Cá Lóc" className="w-16 h-16 rounded-2xl shadow-lg object-cover border border-amber-200" />
                    </div>
                    <CardTitle className="text-2xl font-bold text-amber-900">Quán Bánh Canh Cá Lóc</CardTitle>
                    <p className="text-amber-600 text-sm mt-1">Đăng nhập để tiếp tục</p>
                </CardHeader>
                <CardContent>
                    <form onSubmit={handleSubmit} className="space-y-4 mt-2">
                        <div className="space-y-1">
                            <Label htmlFor="username" className="text-amber-800">Tên đăng nhập</Label>
                            <Input
                                id="username"
                                value={username}
                                onChange={e => setUsername(e.target.value)}
                                placeholder="admin, waiter01, cashier01..."
                                className="border-amber-200 focus:border-amber-500"
                                required
                            />
                        </div>
                        <div className="space-y-1">
                            <Label htmlFor="password" className="text-amber-800">Mật khẩu</Label>
                            <Input
                                id="password"
                                type="password"
                                value={password}
                                onChange={e => setPassword(e.target.value)}
                                placeholder="••••••••"
                                className="border-amber-200 focus:border-amber-500"
                                required
                            />
                        </div>
                        {error && (
                            <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-3 py-2 text-sm">
                                {error}
                            </div>
                        )}
                        <Button
                            type="submit"
                            disabled={loading}
                            className="w-full bg-amber-500 hover:bg-amber-600 text-white font-semibold h-11 rounded-xl"
                        >
                            {loading ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                            Đăng nhập
                        </Button>
                    </form>
                    <div className="mt-4 p-3 bg-amber-50 rounded-lg border border-amber-100 text-xs text-amber-700">
                        <p className="font-medium mb-1">Tài khoản mẫu:</p>
                        <p>admin / admin123</p>
                        <p>waiter01 / waiter123</p>
                        <p>cashier01 / cashier123</p>
                        <p>kitchen01 / kitchen123</p>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}