-- Bug Reports Table for Supabase
-- Run this SQL in your Supabase SQL Editor
-- This allows users to submit bug reports directly from the app

-- Create the bug_reports table
CREATE TABLE IF NOT EXISTS bug_reports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT NOW(),

    -- User info (auto-captured)
    user_id UUID REFERENCES auth.users(id),
    user_email TEXT,
    restaurant_name TEXT,
    location_id UUID,

    -- App info (auto-captured)
    app_version TEXT NOT NULL,
    build_number TEXT,
    os_version TEXT,

    -- User input
    what_were_you_doing TEXT NOT NULL,
    additional_notes TEXT,

    -- Auto-diagnostics
    error_message TEXT,
    stack_trace TEXT,
    diagnostic_json JSONB,

    -- Tracking fields (for admin use)
    status TEXT DEFAULT 'new', -- new, investigating, fixed, wont_fix, duplicate
    priority TEXT DEFAULT 'medium', -- low, medium, high, critical
    resolved_at TIMESTAMPTZ,
    resolution_notes TEXT
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_bug_reports_created_at ON bug_reports(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_bug_reports_status ON bug_reports(status);
CREATE INDEX IF NOT EXISTS idx_bug_reports_user ON bug_reports(user_id);
CREATE INDEX IF NOT EXISTS idx_bug_reports_priority ON bug_reports(priority);

-- Enable Row Level Security
ALTER TABLE bug_reports ENABLE ROW LEVEL SECURITY;

-- Drop existing policies if they exist
DROP POLICY IF EXISTS "Users can create bug reports" ON bug_reports;
DROP POLICY IF EXISTS "Users can view own bug reports" ON bug_reports;
DROP POLICY IF EXISTS "Admin can view all bug reports" ON bug_reports;

-- Policy: Users can insert their own bug reports
CREATE POLICY "Users can create bug reports"
    ON bug_reports FOR INSERT
    TO authenticated
    WITH CHECK (auth.uid() = user_id);

-- Policy: Users can view their own bug reports
CREATE POLICY "Users can view own bug reports"
    ON bug_reports FOR SELECT
    TO authenticated
    USING (auth.uid() = user_id);

-- Policy: Admin can view and manage all bug reports
-- Replace 'your-admin-email@example.com' with your actual admin email
CREATE POLICY "Admin can view all bug reports"
    ON bug_reports FOR ALL
    TO authenticated
    USING (
        auth.email() = 'your-admin-email@example.com'
    );

-- Grant permissions
GRANT ALL ON bug_reports TO authenticated;

-- Comment on table
COMMENT ON TABLE bug_reports IS 'Stores user-submitted bug reports with automatic diagnostics';
COMMENT ON COLUMN bug_reports.what_were_you_doing IS 'User description of what they were trying to do when the bug occurred';
COMMENT ON COLUMN bug_reports.diagnostic_json IS 'Full diagnostic data in JSON format including app state, system info, etc.';
COMMENT ON COLUMN bug_reports.status IS 'Current status: new, investigating, fixed, wont_fix, duplicate';
COMMENT ON COLUMN bug_reports.priority IS 'Priority level: low, medium, high, critical';
