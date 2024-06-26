@echo off
cd /d %~dp0
echo ------------------------
echo Do you want this Repo
echo with version code Initial?
echo ------------------------
pause
git add .
git commit -m "Initial"
git push -u origin main
pause