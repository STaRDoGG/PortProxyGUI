﻿#region + -- IMPORTS -- +

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#endregion

namespace JSE_Utils
{
    public static partial class IPValidation
    {
        #region + -- REGEX -- +

        public static Regex IPv4RegEx = IPv4Pattern();
        public static Regex IPv6RegEx = IPv6Pattern();

        [GeneratedRegex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}")]
        private static partial Regex IPv4Pattern();

        [GeneratedRegex("^(([0-9a-fA-F]{1,4}:){7}([0-9a-fA-F]{1,4}|:))|(([0-9a-fA-F]{1,4}:){0,6}(:[0-9a-fA-F]{1,4}){1,6})$")]
        private static partial Regex IPv6Pattern();

        #endregion
    }

    public static partial class DNS {

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("dnsapi.dll")]
        public static partial bool DnsFlushResolverCache();

        /// <summary>
        /// Flushes the system's DNS cache
        /// </summary>
        /// <param name="bConfirm">[optional: default true] Show a confirmation MessageBox first</param>
        /// <param name="bResult">[optional: default true] Show a sussessful confirmation MessageBox when done</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void FlushCache(bool bConfirm = true, bool bResult = true) {

            // Show dialog asking for Flush confirmation, giving them a change to bail out if it was a misclick.
            if (bConfirm) {

                // If Yes, continue below; if no, bail out of method.
                if (MessageBox.Show("Flush DNS?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No) {

                    Debug.WriteLine("FlushCache: User chose to not flush cache; exiting method.");
                    return;
                }

            }

            // If use either clicked Yes yes above, or bConfirm was False, flush.
            bool status = DnsFlushResolverCache();

            if (status == false) {
                throw new InvalidOperationException("FlushDNS Cache failed.");
            } else {

                // Show a confirmation dialog if desired
                if (bResult) {
                    MessageBox.Show("DNS Flushed!", "Whoosh", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }

        }

    }

    public static class Misc {

        /// <summary>
        /// Standard Process Start function
        /// </summary>
        /// <param name="strFileName">Filename|Program.exe to run</param>
        /// <param name="strArgs">[Optional] Any arguments for the above file|program</param>
        /// <returns>Program output if any, else empty string</returns>
        public static string RunCommand(string strFileName, string strArgs = "", bool bGetOutput = true) {

            string strOutput = string.Empty;

            try {

                using Process p = new();
                {
                    ProcessStartInfo withBlock = p.StartInfo;
                    withBlock.Verb = "runas";
                    withBlock.RedirectStandardOutput = true;
                    withBlock.RedirectStandardError = true;
                    withBlock.FileName = strFileName;
                    withBlock.Arguments = strArgs;
                    withBlock.UseShellExecute = false;
                    withBlock.CreateNoWindow = true;
                }

                p.Start();

                // If we always do the following, the app will lock up when doing something like starting WSL, so it's optional.
                if (bGetOutput) {

                    // Remove any null chars, if exist.
                    strOutput = p.StandardOutput.ReadToEnd().Replace("\0", "");
                    p.WaitForExit();

                }

            } catch (Exception e) {
                Debug.WriteLine("RunCommand() Error: {0}", e.Message);
                throw;
            }

            return strOutput;
        }

        /// <summary>
        /// Checks if a process is running
        /// </summary>
        /// <param name="strProcessName">Name of the process</param>
        /// <returns>True: running; False: not running.</returns>
        public static bool IsProcessRunning(string strProcessName) {

            Process[] p = Process.GetProcessesByName(strProcessName);
            return p.Count() > 0;

        }

        /// <summary>
        /// Creates a custom dialog form so this overall Utils class is more portable
        /// </summary>
        /// <param name="strText">The text to show the user</param>
        /// <param name="strTitle">The title text of the form itself</param>
        /// <returns>DialogResult.OK on "Ok" button click; DialogResult.Cancel on "Cancel" button or X button click.</returns>
        public static DialogResult CustomDialog(string strText, string strTitle) {

            DialogResult result = DialogResult.Cancel;

            using (Form form = new Form()) {

                // Form Props
                form.Text = strTitle;
                form.BackColor = Color.FromArgb(67, 76, 94);
                form.ShowIcon = false;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ShowInTaskbar = false;
                form.FormBorderStyle = FormBorderStyle.FixedSingle;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.Size = new Size(500, 400);

                // Form Controls
                Label lblInfo = new() {
                    BackColor = Color.FromArgb(67, 76, 94),
                    ForeColor = Color.FromArgb(229, 233, 240),
                    Location = new Point(20, 20),
                    Size = new Size(450, 270),
                    AutoSize = false,
                    Font = new Font("Microsoft Sans Serif", 12),
                    Text = strText,
                };

                // Buttons
                Button btnOK = new() {
                    BackColor = Color.FromArgb(46, 52, 64),
                    ForeColor = Color.FromArgb(235, 203, 139),
                    Location = new Point(150, 300),
                    Size = new Size(80, 50),
                    Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    FlatStyle = FlatStyle.Flat,
                };

                // ...
                Button btnCancel = new() {
                    BackColor = Color.FromArgb(46, 52, 64),
                    ForeColor = Color.FromArgb(191, 97, 106),
                    Location = new Point(250, 300),
                    Size = new Size(80, 50),
                    Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    FlatStyle = FlatStyle.Flat,
                };

                // Add the controls
                form.Controls.Add(lblInfo);
                form.Controls.Add(btnOK);
                form.Controls.Add(btnCancel);

                // Set the Cancel button as the focused button, essentially acting as the default button
                form.ActiveControl = btnCancel;

                // Show the form and get the button they clicked
                if (form.ShowDialog() == DialogResult.OK) result = DialogResult.OK;
            }

            // Defaults to false, gets set to true above, only if user clicked the OK button.
            return result;
        }
    }

    public static class Winsock {

        /// <summary>
        /// Resets the Winsock
        /// </summary>
        public static void Reset() {

            string strInfo = string.Format(
                "{0}{3}{3}{1}{3}{3}{2}",
                "This command also resets other networking components like the TCP/IP stack, which can fix various network issues like connectivity problems, DNS resolution issues, and more.",
                "Note that resetting the Winsock should be used as a last resort to fix network issues, as it can affect any running network applications and may cause other issues. If you are experiencing network issues, it may be better to diagnose and fix the root cause of the problem rather than resetting the Winsock.",
                "After running the netsh winsock reset command, you will need to restart your computer for the changes to take effect.",
                Environment.NewLine
            );

            if (Misc.CustomDialog(strInfo, "Winsock Reset") == DialogResult.OK) {

                // Run the winsock reset command
                string strOutput = Misc.RunCommand("netsh.exe", "winsock reset");

                // Report back the result of the reset attempt
                if (strOutput.ToLower().Contains("sucessfully reset")) {

                    MessageBox.Show("You must restart the computer in order to complete the reset.", "Sucessfully reset your Winsock", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                } else {

                    MessageBox.Show(strOutput, "Couldn't reset your Winsock", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                }
            }
        }
    }

    public static class WSL {

        /// <summary>
        /// Checks if WSL is running
        /// </summary>
        /// <param name="bShowMessage">[Optional: default False] will show a messagebox with the result.</param>
        /// <returns>True: WSL is running; False: WSL isn't running.</returns>
        public static bool WSL_IsRunning(bool bShowMessage = false) {

            bool bResult = Misc.IsProcessRunning("wsl");

            if (bShowMessage) {
                MessageBox.Show(string.Format("WSL {0} running. ", bResult ? "is" : "is not"), "WSL", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return bResult;
        }

        /// <summary>
        /// Gets a list of all distros installed, their WSL version, and running state.
        /// </summary>
        /// <returns>Success: WSL Distro info; Fail: Empty string.</returns>
        public static string WSL_GetDistros() {

            // Run the command in Bash
            string strOutput = Misc.RunCommand("wsl.exe", "-l -v --all");

            // TODO: Should be beefed up with validation (make sure proper info was fetched)
            return strOutput.Length > 0 ? strOutput : string.Empty;
        }

        /// <summary>
        /// Gets *all* version info of the currently installed WSL version
        /// </summary>
        /// <returns>Success: WSL Version info; Fail: Empty string.</returns>
        public static string WSL_GetAllVersionInfo() {

            // Run the command
            string strOutput = Misc.RunCommand("wsl.exe", "--version");

            // TODO: Should be beefed up with validation (make sure proper info was fetched)
            return strOutput.Length > 0 ? strOutput : string.Empty;
        }

        /// <summary>
        /// Gets the currently installed WSL version (WSL version *only*)
        /// </summary>
        /// <returns>Success: WSL Version; Fail: Empty string.</returns>
        public static string WSL_GetVersion() {

            // Run the command
            string strOutput = Misc.RunCommand("wsl.exe", "--version");

            string start = "wsl version: ";
            string end = "\r\n";

            // Try to parse out a version number
            int startIndex = strOutput.ToLower().IndexOf(start) + start.Length;
            int endIndex = strOutput.IndexOf(end, startIndex);
            string strResult = strOutput.Substring(startIndex, endIndex - startIndex);

            // TODO: Should be beefed up with validation (make sure an actual version number was fetched)
            return strResult.Length > 0 ? strResult : string.Empty;
        }

        /// <summary>
        /// Fetches the current WSL IP
        /// </summary>
        /// <returns>Success: (string) IP Address; Fail: Empty string</returns>
        public static string WSL_GetIP() {

            // Run the command in Bash
            string strOutput = Misc.RunCommand("bash.exe", string.Format("{0} {1}", "-c", "\"ifconfig eth0 | grep 'inet '\""));

            // Try to parse out a valid IPv4
            string strWSLIP = IPValidation.IPv4RegEx.Match(strOutput).ToString();
            return strWSLIP.Length > 0 ? strWSLIP : string.Empty;

        }

        /// <summary>
        /// Shuts down WSL
        /// </summary>
        /// <param name="bShowResult">[Optional: default True] Shows a messagebox confirming WSL has shut down</param>
        public static void WSL_ShutDown(bool bShowResult = true) {

            if (WSL_IsRunning()) {

                if (MessageBox.Show("Are sure you want to shut WSL down?", "WSL: Shutdown", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {

                    // Run the shutdown command
                    Misc.RunCommand("wsl.exe", "--shutdown");

                    // TODO: Add a timer here to timeout if there's a problem shutting down WSL, so this loop doesn't end up running perpetually.
                    while (WSL_IsRunning()) {
                        // Loop until WSL is confirmed to be shut down, then show them confirmation.
                        // TODO: Add some sort of user feedback here to let them know we're doin work?
                    }

                    if (bShowResult) {
                        MessageBox.Show("WSL is shut down.", "WSL: Shutdown", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
            } else {

                // Ya'll tryna shut down somethin that ain't even runnin!
                MessageBox.Show("WSL isn't running.", "WSL: Shutdown", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Starts WSL
        /// </summary>
        /// <param name="bShowResult">[Optional: default True] Shows a messagebox confirming WSL has started</param>
        public static void WSL_Start(bool bShowResult = true) {

            if (!WSL_IsRunning()) {

                // Run the startup command
                Misc.RunCommand("wsl.exe", string.Empty, false);

                // TODO: Add a timer here to timeout if there's a problem starting up WSL, so this loop doesn't end up running perpetually.
                while (!WSL_IsRunning()) {
                    // Loop until WSL is confirmed to be running, then show them confirmation.
                    // TODO: Add some sort of user feedback here to let them know we're doin work?
                }

                if (bShowResult) {
                    MessageBox.Show("WSL is now running.", "WSL: Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            } else {

                // Ya'll tryna start up somethin that's already runnin!
                MessageBox.Show("WSL is already running.", "WSL: Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        /// <summary>
        /// Restarts WSL
        /// </summary>
        /// <param name="bShowResult">[Optional: default True] Shows a messagebox confirming WSL has restarted</param>
        public static void WSL_Restart(bool bShowResult = true) {

            // Run the shutdown command TODO: need to add confirmation when run fromt he mini menu.
            Misc.RunCommand("wsl.exe", "--shutdown");

            Debug.WriteLine("WSL Restart: Shutdown command sent");

            while (WSL_IsRunning()) {
                // Loop until WSL is confirmed to be shut down, then start WSL again.
                // TODO: Add some sort of user feedback here to let them know we're doin work?
            }

            Debug.WriteLine("WSL Restart: WSL has shutdown");

            // Run the startup command
            Misc.RunCommand("wsl.exe", string.Empty, false);

            Debug.WriteLine("WSL Restart: Startup command sent");

            while (!WSL_IsRunning()) {
                // Loop until WSL is confirmed to be running, then show them confirmation.
                // TODO: Add some sort of user feedback here to let them know we're doin work?
            }

            Debug.WriteLine("WSL Restart: WSL has restarted");

            if (bShowResult) {
                MessageBox.Show("WSL has restarted.", "WSL: Restart", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
