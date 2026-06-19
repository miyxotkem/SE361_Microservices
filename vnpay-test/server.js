const express = require('express');
const { VNPay, ignoreLogger, ProductCode, VnpLocale, dateFormat } = require('vnpay');

const app = express();
const port = 3005;

// =====================================================
// CẤU HÌNH VNPAY - THAY ĐỔI THÔNG TIN CỦA BẠN Ở ĐÂY
// =====================================================
const vnpay = new VNPay({
    tmnCode: 'FEJ1PW2Z',
    secureSecret: 'G8WFG003FREX57T1ABPOFC6J16GRWV8F',
    vnpayHost: 'https://sandbox.vnpayment.vn',
    testMode: true,
    hashAlgorithm: 'SHA512',
    loggerFn: ignoreLogger,
});

// =====================================================
// API TẠO LINK THANH TOÁN
// =====================================================
app.post('/api/create-qr', async (req, res) => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);

    const vnpayResponse = await vnpay.buildPaymentUrl({
        vnp_Amount: 50000,
        vnp_IpAddr: '127.0.0.1',
        vnp_TxnRef: '123456',
        vnp_OrderInfo: '123456',
        vnp_OrderType: ProductCode.Other,
        vnp_ReturnUrl: 'http://localhost:3000/api/check-payment-vnpay',
        vnp_Locale: VnpLocale.VN,
        vnp_CreateDate: dateFormat(new Date()),
        vnp_ExpireDate: dateFormat(tomorrow),
    });

    return res.status(201).json(vnpayResponse);
});

// =====================================================
// API KIỂM TRA KẾT QUẢ THANH TOÁN (ReturnUrl)
// =====================================================
app.get('/api/check-payment-vnpay', (req, res) => {
    console.log('VNPay Return:', req.query);

    const vnp_ResponseCode = req.query.vnp_ResponseCode;
    const vnp_TransactionStatus = req.query.vnp_TransactionStatus;

    if (vnp_ResponseCode === '00' && vnp_TransactionStatus === '00') {
        res.send(`
            <h1 style="color: green;">✅ Thanh toán thành công!</h1>
            <pre>${JSON.stringify(req.query, null, 2)}</pre>
        `);
    } else {
        res.send(`
            <h1 style="color: red;">❌ Thanh toán thất bại!</h1>
            <p>Response Code: ${vnp_ResponseCode}</p>
            <pre>${JSON.stringify(req.query, null, 2)}</pre>
        `);
    }
});

app.listen(port, () => {
    console.log(`\n========================================`);
    console.log(`VNPay Test Server running on port ${port}`);
    console.log(`========================================`);
    console.log(`\nĐể test, mở terminal mới và chạy:`);
    console.log(`  curl -X POST http://localhost:${port}/api/create-qr`);
    console.log(`\nHoặc dùng PowerShell:`);
    console.log(`  Invoke-RestMethod -Uri "http://localhost:${port}/api/create-qr" -Method Post`);
    console.log(`\nSau đó mở link trả về trong trình duyệt để thanh toán thử.`);
    console.log(`\nThẻ test VNPay Sandbox:`);
    console.log(`  Số thẻ: 9704198526191432198`);
    console.log(`  Tên:    NGUYEN VAN A`);
    console.log(`  Ngày:   07/15`);
    console.log(`  OTP:    123456`);
    console.log(`========================================\n`);
});
