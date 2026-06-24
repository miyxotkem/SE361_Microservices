-- DDL Script for creating the users table on Supabase (PostgreSQL)
-- You can run this in the Supabase SQL Editor:

CREATE TABLE IF NOT EXISTS users (
    id VARCHAR(128) PRIMARY KEY,
    email VARCHAR(256) NOT NULL,
    password_hash VARCHAR(256) NULL,
    full_name VARCHAR(256) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'Student',
    phone_number VARCHAR(20) NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_blocked BOOLEAN NOT NULL DEFAULT FALSE,
    profile_image_url VARCHAR(512) NULL
);

-- Create a unique index on email
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users(email);
