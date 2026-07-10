const fs = require('fs');
const envPath = 'd:\\SE361\\SE361_Microservices\\.env';

let envContent = fs.readFileSync(envPath, 'utf8');

// Path to JSON files
const filesToUpdate = [
    {
        jsonPath: 'd:\\SE361\\SE361_Microservices\\firebase\\exam-db-8e1b4-firebase-adminsdk-fbsvc-e7c027ca2c.json',
        envKey: 'FIREBASE_CREDENTIALS_JSON_EXAM'
    },
    {
        jsonPath: 'd:\\SE361\\SE361_Microservices\\firebase\\comment-db-10f06-firebase-adminsdk-fbsvc-5a6e0e3894.json',
        envKey: 'FIREBASE_CREDENTIALS_JSON_COMMENT'
    }
];

for (const { jsonPath, envKey } of filesToUpdate) {
    if (fs.existsSync(jsonPath)) {
        const jsonContent = fs.readFileSync(jsonPath, 'utf8');
        const flattenedJson = JSON.stringify(JSON.parse(jsonContent));
        const regex = new RegExp(`^${envKey}=.*$`, 'm');
        envContent = envContent.replace(regex, `${envKey}=${flattenedJson}`);
        console.log(`Successfully updated ${envKey}`);
    } else {
        console.log(`File not found: ${jsonPath}`);
    }
}

fs.writeFileSync(envPath, envContent, 'utf8');
console.log('Finished updating .env');
