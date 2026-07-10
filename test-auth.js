const { GoogleAuth } = require('google-auth-library');

async function main() {
    try {
        const auth = new GoogleAuth({
            credentials: JSON.parse(process.env.FIREBASE_CREDENTIALS_JSON_COURSE),
            scopes: 'https://www.googleapis.com/auth/cloud-platform'
        });
        const client = await auth.getClient();
        const token = await client.getAccessToken();
        console.log('Successfully got token:', token.token ? 'YES' : 'NO');
    } catch (e) {
        console.error('Error getting token:', e.message);
    }
}
main();
