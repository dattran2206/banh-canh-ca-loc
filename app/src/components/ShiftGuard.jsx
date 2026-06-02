import React from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Clock, PlayCircle } from 'lucide-react';

export default function ShiftGuard({ children }) {
    const { currentShift, startShift, currentUser } = useAppAuth();

    if (currentUser?.role === 'admin') {
        return <>{children}</>;
    }

    if (!currentShift) {
        return (
            <div className="min-h-screen bg-amber-50 flex items-center justify-center p-4">
                <Card className="w-full max-w-sm shadow-xl border-amber-200">
                    <CardContent className="pt-8 pb-6 text-center">
                        <div className="flex justify-center mb-4">
                            <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center">
                                <Clock className="w-8 h-8 text-amber-600" />
                            </div>
                        </div>
                        <h2 className="text-xl font-bold text-amber-900 mb-1">Xin chào, {currentUser?.fullName}!</h2>
                        <p className="text-amber-600 text-sm mb-6">Bạn chưa bắt đầu ca làm việc hôm nay.</p>
                        <Button
                            onClick={startShift}
                            className="bg-amber-500 hover:bg-amber-600 text-white font-semibold h-12 px-8 rounded-xl text-base"
                        >
                            <PlayCircle className="w-5 h-5 mr-2" />
                            Bắt đầu ca làm việc
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return <>{children}</>;
}