# How to Build the Installer with Credentials

## Security Notice

The actual installer script with credentials is **NOT** committed to version control for security reasons.

## Building the Installer

### Step 1: Create Your Local Installer Script

Copy the template and add your credentials:

```bash
cd C:\Projects\installer
copy Desktop Food CostSetup.iss.template Desktop Food CostSetup.iss
```

### Step 2: Add Your Credentials

Open `Desktop Food CostSetup.iss` in a text editor and replace the placeholders:

**Line ~202:** Replace `YOUR_SUPABASE_URL_HERE` with your Supabase URL
```pascal
RegWriteStringValue(HKCU, 'Environment', 'SUPABASE_URL', 'https://YOUR_PROJECT.supabase.co')
```

**Line ~203:** Replace `YOUR_SUPABASE_ANON_KEY_HERE` with your Supabase anon key
```pascal
RegWriteStringValue(HKCU, 'Environment', 'SUPABASE_ANON_KEY', 'eyJhbGc...')
```

**Line ~225:** Replace `YOUR_USDA_API_KEY_HERE` with your USDA API key
```pascal
RegWriteStringValue(HKCU, 'Environment', 'USDA_API_KEY', 'your-usda-key')
```

### Step 3: Get Your Credentials

#### Supabase Credentials:
1. Go to https://supabase.com/dashboard
2. Select your project
3. Go to Settings → API
4. Copy:
   - Project URL (SUPABASE_URL)
   - anon/public key (SUPABASE_ANON_KEY)

#### USDA API Key:
1. Go to https://fdc.nal.usda.gov/api-key-signup.html
2. Sign up for a free API key
3. Copy your API key

### Step 4: Build the Installer

1. Publish the application:
   ```bash
   cd C:\Projects
   dotnet publish Desktop Food Cost.Desktop/Desktop Food Cost.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o publish/win-x64
   ```

2. Compile the installer with Inno Setup:
   ```bash
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "C:\Projects\installer\Desktop Food CostSetup.iss"
   ```

3. The installer will be created at:
   ```
   C:\Projects\installer\output\Desktop Food CostSetup-1.0.0-x64-Complete.exe
   ```

## Important Notes

- ✅ `Desktop Food CostSetup.iss` is in `.gitignore` and will NOT be committed
- ✅ `Desktop Food CostSetup.iss.template` is the version-controlled template
- ✅ The compiled `.exe` installer can be uploaded to GitHub Releases
- ⚠️ Never commit the actual `Desktop Food CostSetup.iss` file with credentials!

## Rotating Credentials

If credentials are exposed:

1. **Rotate the Supabase key:**
   - Go to Supabase Dashboard → Settings → API
   - Reset the anon/public key

2. **Update Desktop Food CostSetup.iss** with the new key

3. **Rebuild the installer** and distribute the new version

4. **Update environment variables** on all machines using the app
