const { VNPay, ProductCode, VnpLocale, dateFormat } = require('vnpay');

const vnpay = new VNPay({
    tmnCode: 'FEJ1PW2Z',
    secureSecret: 'G8WFG003FREX57T1ABPOFC6J16GRWV8F',
    vnpayHost: 'https://sandbox.vnpayment.vn',
    testMode: true,
    hashAlgorithm: 'SHA512',
});

const vnpayResponse = vnpay.buildPaymentUrl({
    vnp_Amount: 50000,
    vnp_IpAddr: '127.0.0.1',
    vnp_TxnRef: '123456',
    vnp_OrderInfo: '123456',
    vnp_OrderType: ProductCode.Other,
    vnp_ReturnUrl: 'http://localhost:3000/api/check-payment-vnpay',
    vnp_Locale: VnpLocale.VN,
    vnp_CreateDate: 20260617004849, // pass as number
    vnp_ExpireDate: 20260618004849, // pass as number
});

console.log(vnpayResponse);
