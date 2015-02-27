// Copyright (c) 2014-2015 Wolfgang Borgsmüller
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// 1. Redistributions of source code must retain the above copyright 
//    notice, this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright 
//    notice, this list of conditions and the following disclaimer in the 
//    documentation and/or other materials provided with the distribution.
// 
// 3. Neither the name of the copyright holder nor the names of its 
//    contributors may be used to endorse or promote products derived 
//    from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE 
// COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS 
// OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND 
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
// USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.



using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chromium;
using Chromium.Remote;

namespace Chromium.WebBrowser {

    public delegate void OnBeforeCfxInitializeEventHandler(CfxSettings settings, CfxBrowserProcessHandler processHandler, out CfxOnBeforeCommandLineProcessingEventHandler onBeforeCommandLineProcessingEventHandler);

    public class ChromiumWebBrowser : Control {

        private static CfxBrowserSettings defaultBrowserSettings;
        public static CfxBrowserSettings DefaultBrowserSettings {
            get {
                if(defaultBrowserSettings == null) {
                    if(!CfxRuntime.LibrariesLoaded)
                        CfxRuntime.LoadLibraries();
                    defaultBrowserSettings = new CfxBrowserSettings();
                }
                return defaultBrowserSettings;
            }
        }

        /// <summary>
        /// Gives the application an opportunity to change initialization settings,
        /// subscribe to browser process handler events and provide 
        /// an event handler for OnBeforeCommandLineProcessing events.
        /// </summary>
        public static event OnBeforeCfxInitializeEventHandler OnBeforeCfxInitialize;
        internal static void RaiseOnBeforeCfxInitialize(CfxSettings settings, CfxBrowserProcessHandler processHandler, out CfxOnBeforeCommandLineProcessingEventHandler onBeforeCommandLineProcessingEventHandler) {
            var handler = OnBeforeCfxInitialize;
            if(handler != null)
                handler(settings, processHandler, out onBeforeCommandLineProcessingEventHandler);
            else
                onBeforeCommandLineProcessingEventHandler = null;
        }
        

        /// <summary>
        /// Initialize the ChromiumWebBrowser and ChromiumFX libraries.
        /// The application can change initialization settings by handling
        /// the OnBeforeCfxInitialize event.
        /// </summary>
        public static void Initialize() {
            BrowserProcess.Initialize();
        }

        /// <summary>
        /// This function should be called on the main application thread to shut down
        /// the CEF browser process before the application exits.
        /// </summary>
        public static void Shutdown() {
            CfxRuntime.Shutdown();
        }


        public static CfxBrowserProcessHandler BrowserProcessHandler {
            get {
                return BrowserProcess.processHandler;
            }
        }

        private static readonly Dictionary<int, ChromiumWebBrowser> browsers = new Dictionary<int, ChromiumWebBrowser>();
        internal static ChromiumWebBrowser GetBrowser(int id) {
            ChromiumWebBrowser wb;
            ChromiumWebBrowser.browsers.TryGetValue(id, out wb);
            return wb;
        }

        private BrowserClient client;

        public CfxBrowser Browser { get; private set; }
        public CfxBrowserHost BrowserHost { get; private set; }

        private readonly object browserSyncRoot = new object();
        private IntPtr browserWindowHandle;
        private int browserId;

        internal readonly Dictionary<string, List<JSFunction>> frameJSFunctions = new Dictionary<string, List<JSFunction>>();
        internal readonly List<JSFunction> mainFrameJSFunctions = new List<JSFunction>();
        internal readonly Dictionary<string, WebResource> webResources = new Dictionary<string, WebResource>();

        internal RenderProcess remoteProcess;
        internal CfrBrowser remoteBrowser;

        private string initialUrl;

        /// <summary>
        /// Creates a ChromiumWebBrowser object with about:blank as initial URL.
        /// The underlying CfxBrowser is created immediately with null
        /// as CfxRequestContext.
        /// </summary>
        public ChromiumWebBrowser() : this(null, true) {}

        /// <summary>
        /// Creates a ChromiumWebBrowser object with about:blank as initial URL.
        /// If createImmediately is true, then the underlying CfxBrowser is 
        /// created immediately with null as CfxRequestContext.
        /// </summary>
        /// <param name="createImmediately"></param>
        public ChromiumWebBrowser(bool createImmediately) : this(null, createImmediately) {}

        /// <summary>
        /// Creates a ChromiumWebBrowser object with the given initial URL.
        /// The underlying CfxBrowser is created immediately with null
        /// as CfxRequestContext.
        /// </summary>
        public ChromiumWebBrowser(string initialUrl) : this(initialUrl, true) { }

        /// <summary>
        /// Creates a ChromiumWebBrowser object with the given initial URL.
        /// If createImmediately is true, then the underlying CfxBrowser is 
        /// created immediately with null as CfxRequestContext.
        /// </summary>
        public ChromiumWebBrowser(string initialUrl, bool createImmediately) {

            if(BrowserProcess.initialized) {

                SetStyle(ControlStyles.ContainerControl
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.FixedWidth
                    | ControlStyles.FixedHeight
                    | ControlStyles.StandardClick
                    | ControlStyles.StandardDoubleClick
                    | ControlStyles.UserMouse
                    | ControlStyles.SupportsTransparentBackColor
                    | ControlStyles.EnableNotifyMessage
                    | ControlStyles.DoubleBuffer
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.UseTextForAccessibility
                    | ControlStyles.Opaque
                    , false);

                SetStyle(ControlStyles.UserPaint
                    | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.CacheText
                    | ControlStyles.Selectable
                    , true);

                if(initialUrl == null)
                    this.initialUrl = "about:blank";
                else
                    this.initialUrl = initialUrl;

                client = new BrowserClient(this);

                if(createImmediately)
                    CreateBrowser();

            } else {
                BackColor = System.Drawing.Color.White;
                Width = 200;
                Height = 160;
                var label = new Label();
                label.AutoSize = true; 
                label.Text = "ChromiumWebBrowser";
                label.Parent = this;
            }
        }


        public void CreateBrowser() {
            CreateBrowser(null);
        }

        public void CreateBrowser(CfxRequestContext requestContext) {

            var windowInfo = new CfxWindowInfo();
            windowInfo.Height = Height > 0 ? Height : 500;
            windowInfo.Width = Width > 0 ? Width : 500;
            windowInfo.X = 0;
            windowInfo.Y = 0;
            windowInfo.Style = (int)(WindowStyles.WS_CHILD | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_VISIBLE);
            windowInfo.ParentWindow = this.Handle;

            if(!CfxBrowserHost.CreateBrowser(windowInfo, client, initialUrl, DefaultBrowserSettings, requestContext))
                throw new CfxException("Failed to create browser instance.");
        }

        
        /// <summary>
        /// Returns the context menu handler for this browser.
        /// </summary>
        public CfxContextMenuHandler ContextMenuHandler { get { return client.ContextMenuHandler; } }

        /// <summary>
        /// Returns the life span handler for this browser.
        /// </summary>
        public CfxLifeSpanHandler LifeSpanHandler { get { return client.lifeSpanHandler; } }

        /// <summary>
        /// Returns the load handler for this browser.
        /// </summary>
        public CfxLoadHandler LoadHandler { get { return client.LoadHandler; } }

        /// <summary>
        /// Returns the request handler for this browser.
        /// Do not set the return value in the GetResourceHandler event for URLs
        /// with associated WebResources (see also SetWebResource).
        /// </summary>
        public CfxRequestHandler RequestHandler { get { return client.requestHandler; } }

        /// <summary>
        /// Returns the display handler for this browser.
        /// </summary>
        public CfxDisplayHandler DisplayHandler { get { return client.DisplayHandler; } }


        /// <summary>
        /// Returns the URL currently loaded in the main frame.
        /// </summary>
        public System.Uri Url { get { return new System.Uri(Browser.MainFrame.Url); } }

        /// <summary>
        /// Returns true if the browser is currently loading.
        /// </summary>
        public bool IsLoading { get { return Browser == null ? false : Browser.IsLoading; } }

        /// <summary>
        /// Returns true if the browser can navigate backwards.
        /// </summary>
        public bool CanGoBack { get { return Browser == null ? false : Browser.CanGoBack; } }

        /// <summary>
        /// Returns true if the browser can navigate forwards.
        /// </summary>
        public bool CanGoForward { get { return Browser == null ? false : Browser.CanGoForward; } }

        /// <summary>
        /// Navigate backwards.
        /// </summary>
        public void GoBack() { if(Browser != null) Browser.GoBack(); }

        /// <summary>
        /// Navigate forwards.
        /// </summary>
        public void GoForward() { if(Browser != null) Browser.GoForward(); }

        /// <summary>
        /// Load the specified |url| into the main frame.
        /// </summary>
        public void LoadUrl(string url) {
            if(Browser != null)
                Browser.MainFrame.LoadUrl(url);
            else {
                lock(browserSyncRoot) {
                    if(Browser != null) {
                        Browser.MainFrame.LoadUrl(url);
                    } else {
                        m_loadUrlDeferred = url;
                    }
                }
            }
        }

        /// <summary>
        /// Load the contents of |stringVal| with the specified dummy |url|. |url|
        /// should have a standard scheme (for example, http scheme) or behaviors like
        /// link clicks and web security restrictions may not behave as expected.
        /// </summary>
        public void LoadString(string stringVal, string url) {
            if(Browser != null) {
                Browser.MainFrame.LoadString(stringVal, url);
            } else {
                lock(browserSyncRoot) {
                    if(Browser != null) {
                        Browser.MainFrame.LoadString(stringVal, url);
                    } else {
                        m_loadUrlDeferred = url;
                        m_loadStringDeferred = stringVal;
                    }
                }
            }
        }

        /// <summary>
        /// Load the contents of |stringVal| with dummy url about:blank.
        /// </summary>
        public void LoadString(string stringVal) {
            LoadString(stringVal, "about:blank");
        }


        private int findId;
        private string currentFindText;
        
        /// <summary>
        /// Search for |searchText|. |forward| indicates whether to search forward or
        /// backward within the page. |matchCase| indicates whether the search should
        /// be case-sensitive.
        /// </summary>
        public void Find(string searchText, bool forward, bool matchCase) {
            var findNext = currentFindText == searchText;
            if(!findNext) {
                currentFindText = searchText;
                ++findId;
            }
            BrowserHost.Find(findId, searchText, forward, matchCase, findNext);
        }

        /// <summary>
        /// Search for |searchText|. |forward| indicates whether to search forward or
        /// backward within the page. The search will be case-insensitive.
        /// </summary>
        public void Find(string searchText, bool forward) {
            Find(searchText, forward, false);
        }

        /// <summary>
        /// Search for |searchText|. The search will be forward and case-insensitive.
        /// </summary>
        public void Find(string searchText) {
            Find(searchText, true, false);
        }


        /// <summary>
        /// Execute a string of javascript code in the browser's main frame.
        /// Execution is asynchronous, this function returns immediately.
        /// Returns false if the browser has not yet been created.
        /// </summary>
        public bool ExecuteJavascript(string code) {
            if(Browser != null) {
                Browser.MainFrame.ExecuteJavaScript(code, null, 0);
                return true;
            } else {
                return false;
            }
        }

        
        /// <summary>
        /// Evaluate a string of javascript code in the browser's main frame.
        /// Evaluation is done asynchronously in the render process.
        /// Returns false if the remote browser is currently unavailable.
        /// If this function returns false, then |callback| will not be called. Otherwise,
        /// |callback| will be called on the thread that owns this browser control's 
        /// underlying window handle, preserving affinity to the renderer thread.
        /// Use with care:
        /// The callback may never be called if the render process gets killed prematurely.
        /// Otherwise, the returned CfrV8Value may be null if evaluation in the render process fails.
        /// Do not block the callback since it blocks the render thread.
        /// </summary>
        /// <param name="code">The javascript code to evaluate.</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool EvaluateJavascript(string code, Action<CfrV8Value, CfrV8Exception> callback) {
            var rb = remoteBrowser;
            if(rb == null) return false;
            try {
                var taskRunner = CfrTaskRunner.GetForThread(rb.RemoteRuntime, CfxThreadId.Renderer);
                var task = new EvaluateTask(this, code, callback);
                taskRunner.PostTask(task);
                return true;
            } catch(System.IO.IOException) {
                return false;
            }
        }

        private class EvaluateTask : CfrTask {
            
            ChromiumWebBrowser wb;
            string code;
            Action<CfrV8Value, CfrV8Exception> callback;

            internal EvaluateTask(ChromiumWebBrowser wb, string code, Action<CfrV8Value, CfrV8Exception> callback)
                : base(wb.remoteProcess.remoteRuntime) {
                this.wb = wb;
                this.code = code;
                this.callback = callback;
                this.Execute += Task_Execute;
            }

            void Task_Execute(object sender, CfrEventArgs e) {
                try {
                    CfrV8Value retval;
                    CfrV8Exception ex;
                    var context = wb.remoteBrowser.MainFrame.V8Context;
                    var result = context.Eval(code, out retval, out ex);
                    if(result) {
                        wb.Invoke((MethodInvoker)(() => { callback(retval, ex); }));
                    } else {
                        wb.Invoke((MethodInvoker)(() => { callback(null, null); }));
                    }
                    
                } catch {
                    wb.Invoke((MethodInvoker)(() => { callback(null, null); }));
                }
            }
        }

        
        /// <summary>
        /// Add a JS Function to the main frame's global object.
        /// The function will be available after the next time a
        /// V8 context is created in the render process.
        /// </summary>
        public void AddGlobalJSFunction(JSFunction globalFunction) {
            if(globalFunction.Browser != null) {
                throw new CfxException("This JSFunction object already belongs to a browser.");
            }
            globalFunction.Browser = this;
            mainFrameJSFunctions.Add(globalFunction);
        }

        /// <summary>
        /// Add a JS Function to the named frame's global object.
        /// The function will be available after the next time a
        /// V8 context is created for a frame with this name in the render process.
        /// </summary>
        public void AddGlobalJSFunction(string frameName, JSFunction globalFunction) {
            if(globalFunction.Browser != null) {
                throw new CfxException("This JSFunction object already belongs to a browser.");
            }
            List<JSFunction> list;
            if(!frameJSFunctions.TryGetValue(frameName, out list)) {
                list = new List<JSFunction>();
                frameJSFunctions.Add(frameName, list);
            }
            globalFunction.Browser = this;
            list.Add(globalFunction);
        }

        /// <summary>
        /// Add a JS Function to the main frame's global object.
        /// The function will be available after the next time a
        /// V8 context is created in the render process.
        /// The function is executed on the thread that owns this browser control's 
        /// underlying window handle. Preserves affinity to the original thread.
        /// </summary>
        public JSFunction AddGlobalJSFunction(string functionName) {
            var f = new JSFunction(functionName, this);
            AddGlobalJSFunction(f);
            return f;
        }

        /// <summary>
        /// Add a JS Function to the main frame's global object.
        /// The function will be available after the next time a
        /// V8 context is created in the render process.
        /// If executeOnUiThread is true, then the function is 
        /// executed on the thread that owns this browser control's 
        /// underlying window handle. Preserves affinity to the original thread.
        /// </summary>
        public JSFunction AddGlobalJSFunction(string functionName, bool executeOnUiThread) {
            var f = new JSFunction(functionName, executeOnUiThread ? this : null);
            AddGlobalJSFunction(f);
            return f;
        }

        /// <summary>
        /// Add a JS Function to the named frame's global object.
        /// The function will be available after the next time a
        /// V8 context is created in the render process.
        /// The function is executed on the thread that owns this browser control's 
        /// underlying window handle. Preserves affinity to the original thread.
        /// </summary>
        public JSFunction AddGlobalJSFunction(string frameName, string functionName) {
            var f = new JSFunction(functionName, this);
            AddGlobalJSFunction(frameName, f);
            return f;
        }

        /// <summary>
        /// Visit the remote browser object.
        /// Returns false if the remote browser is currently unavailable.
        /// If this function returns false, then |callback| will not be called. Otherwise,
        /// |callback| will be called on the thread that owns this browser control's 
        /// underlying window handle, preserving affinity to the renderer thread.
        /// Use with care:
        /// The callback may never be called if the render process gets killed prematurely.
        /// Do not keep a reference to the remote browser after returning from the callback.
        /// Do not block the callback since it blocks the render thread.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool VisitRemoteBrowser(Action<CfrBrowser> callback) {
            var rb = remoteBrowser;
            if(rb == null) return false;
            try {
                var taskRunner = CfrTaskRunner.GetForThread(rb.RemoteRuntime, CfxThreadId.Renderer);
                var task = new VisitRemoteBrowserTask(this, callback);
                taskRunner.PostTask(task);
                return true;
            } catch(System.IO.IOException) {
                return false;
            }
        }

        private class VisitRemoteBrowserTask : CfrTask {

            ChromiumWebBrowser wb;
            Action<CfrBrowser> callback;

            internal VisitRemoteBrowserTask(ChromiumWebBrowser wb, Action<CfrBrowser> callback)
                : base(wb.remoteProcess.remoteRuntime) {
                this.wb = wb;
                this.callback = callback;
                this.Execute += Task_Execute;
            }

            void Task_Execute(object sender, CfrEventArgs e) {
                wb.Invoke((MethodInvoker)(() => { callback(wb.remoteBrowser); }));
            }
        }



        /// <summary>
        /// Visit the DOM in the remote browser's main frame.
        /// Returns false if the remote browser is currently unavailable.
        /// If this function returns false, then |callback| will not be called. Otherwise,
        /// |callback| will be called on the thread that owns this browser control's 
        /// underlying window handle, preserving affinity to the renderer thread.
        /// The document object passed to the callback represents a snapshot 
        /// of the DOM at the time the callback is executed.
        /// DOM objects are only valid for the scope of the callback. Do not
        /// keep references to or attempt to access any DOM objects outside the scope
        /// of the callback.
        /// Use with care:
        /// The callback may never be called if the render process gets killed prematurely.
        /// Do not keep a reference to the remote DOM or remote browser object after returning from the callback.
        /// Do not block the callback since it blocks the renderer thread.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool VisitDom(Action<CfrDomDocument, CfrBrowser> callback) {
            var rb = remoteBrowser;
            if(rb == null) return false;
            try {
                var taskRunner = CfrTaskRunner.GetForThread(rb.RemoteRuntime, CfxThreadId.Renderer);
                var task = new VisitDomTask(this, callback);
                taskRunner.PostTask(task);
                return true;
            } catch(System.IO.IOException) {
                return false;
            }
        }

        private class VisitDomTask : CfrTask {

            ChromiumWebBrowser wb;
            Action<CfrDomDocument, CfrBrowser> callback;
            CfrDomVisitor visitor;

            internal VisitDomTask(ChromiumWebBrowser wb, Action<CfrDomDocument, CfrBrowser> callback)
                : base(wb.remoteProcess.remoteRuntime) {
                this.wb = wb;
                this.callback = callback;
                this.Execute += Task_Execute;
                visitor = new CfrDomVisitor(wb.remoteBrowser.RemoteRuntime);
                visitor.Visit += visitor_Visit;
            }

            void Task_Execute(object sender, CfrEventArgs e) {
                wb.remoteBrowser.MainFrame.VisitDom(visitor);
            }

            void visitor_Visit(object sender, CfrDomVisitorVisitEventArgs e) {
                wb.Invoke((MethodInvoker)(() => { callback(e.Document, wb.remoteBrowser); }));
            }

        }


        /// <summary>
        /// Set a resource to be used for the specified URL.
        /// Note that those resources are kept in the memory.
        /// If you need bulk handling of web resources,
        /// subscribing to RequestHandler.GetResourceHandler
        /// might be a better choice.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="resource"></param>
        public void SetWebResource(string url, WebResource resource) {
            webResources[url]= resource;
        }

        /// <summary>
        /// Remove a resource previously set for the specified URL.
        /// </summary>
        /// <param name="url"></param>
        public void RemoveWebResource(string url) {
            webResources.Remove(url);
        }
        
        
        /// <summary>
        /// Raised after the CfxBrowser object for this WebBrowser has been created.
        /// </summary>
        public event CfxOnAfterCreatedEventHandler OnAfterCreated;
        
        /// <summary>
        /// Called when the loading state has changed. This callback will be executed
        /// twice -- once when loading is initiated either programmatically or by user
        /// action, and once when loading is terminated due to completion, cancellation
        /// of failure.
        /// The event is executed on the thread that owns this browser control's 
        /// underlying window handle.
        /// </summary>
        public event CfxOnLoadingStateChangeEventHandler OnLoadingStateChange {
            add {
                lock(browserSyncRoot) {
                    if(m_OnLoadingStateChange == null)
                        client.LoadHandler.OnLoadingStateChange += RaiseOnLoadingStateChange;
                    m_OnLoadingStateChange += value;
                }
            }
            remove {
                lock(browserSyncRoot) {
                    m_OnLoadingStateChange -= value;
                    if(m_OnLoadingStateChange == null)
                        client.LoadHandler.OnLoadingStateChange -= RaiseOnLoadingStateChange;
                }
            }
        }

        private CfxOnLoadingStateChangeEventHandler m_OnLoadingStateChange;
        private void RaiseOnLoadingStateChange(object sender, CfxOnLoadingStateChangeEventArgs e) {
            var handler = m_OnLoadingStateChange;
            if(handler != null) {
                Invoke((MethodInvoker)(() => { handler(this, e); }));
            }
        }


        /// <summary>
        /// Called before a context menu is displayed. |params| provides information
        /// about the context menu state. |model| initially contains the default
        /// context menu. The |model| can be cleared to show no context menu or
        /// modified to show a custom menu. Do not keep references to |params| or
        /// |model| outside of this callback.
        /// The event is executed on the thread that owns this browser control's 
        /// underlying window handle.
        /// </summary>
        public event CfxOnBeforeContextMenuEventHandler OnBeforeContextMenu {
            add {
                lock(browserSyncRoot) {
                    if(m_OnBeforeContextMenu == null)
                        client.ContextMenuHandler.OnBeforeContextMenu += RaiseOnBeforeContextMenu;
                    m_OnBeforeContextMenu += value;
                }
            }
            remove {
                lock(browserSyncRoot) {
                    m_OnBeforeContextMenu -= value;
                    if(m_OnBeforeContextMenu == null)
                        client.ContextMenuHandler.OnBeforeContextMenu -= RaiseOnBeforeContextMenu;
                }
            }
        }

        private CfxOnBeforeContextMenuEventHandler m_OnBeforeContextMenu;
        private void RaiseOnBeforeContextMenu(object sender, CfxOnBeforeContextMenuEventArgs e) {
            var handler = m_OnBeforeContextMenu;
            if(handler != null) {
                Invoke((MethodInvoker)(() => { handler(this, e); }));
            }
        }


        /// <summary>
        /// Called to execute a command selected from the context menu. Return true (1)
        /// if the command was handled or false (0) for the default implementation. See
        /// cef_menu_id_t for the command ids that have default implementations. All
        /// user-defined command ids should be between MENU_ID_USER_FIRST and
        /// MENU_ID_USER_LAST. |params| will have the same values as what was passed to
        /// on_before_context_menu(). Do not keep a reference to |params| outside of
        /// this callback.
        /// The event is executed on the thread that owns this browser control's 
        /// underlying window handle.
        /// </summary>
        public event CfxOnContextMenuCommandEventHandler OnContextMenuCommand {
            add {
                lock(browserSyncRoot) {
                    if(m_OnContextMenuCommand == null)
                        client.ContextMenuHandler.OnContextMenuCommand += RaiseOnContextMenuCommand;
                    m_OnContextMenuCommand += value;
                }
            }
            remove {
                lock(browserSyncRoot) {
                    m_OnContextMenuCommand -= value;
                    if(m_OnContextMenuCommand == null)
                        client.ContextMenuHandler.OnContextMenuCommand -= RaiseOnContextMenuCommand;
                }
            }
        }

        private CfxOnContextMenuCommandEventHandler m_OnContextMenuCommand;
        private void RaiseOnContextMenuCommand(object sender, CfxOnContextMenuCommandEventArgs e) {
            var handler = m_OnContextMenuCommand;
            if(handler != null) {
                Invoke((MethodInvoker)(() => { handler(this, e); }));
            }
        }

        //TODO: create more browser events on the client handler events, invoking the UI thread.


                
        private string m_loadUrlDeferred;
        private string m_loadStringDeferred;

        internal void OnBrowserCreated(CfxOnAfterCreatedEventArgs e) {
            Browser = e.Browser;
            BrowserHost = Browser.Host;
            browserWindowHandle = BrowserHost.WindowHandle;
            browserId = Browser.Identifier;
            browsers.Add(browserId, this);
            SetWindowPos(browserWindowHandle, IntPtr.Zero, 0, 0, Width, Height, SWP_NOMOVE | SWP_NOZORDER);
            var handler = OnAfterCreated;
            if(handler != null)
                Invoke((MethodInvoker)(() => { handler(this, e); }));
            System.Threading.ThreadPool.QueueUserWorkItem(AfterSetBrowserTasks);
        }

        private void AfterSetBrowserTasks(object state) {
            lock(browserSyncRoot) {
                if(m_loadUrlDeferred != null) {
                    if(m_loadStringDeferred != null) {
                        Browser.MainFrame.LoadString(m_loadStringDeferred, m_loadUrlDeferred);
                    } else {
                        Browser.MainFrame.LoadUrl(m_loadUrlDeferred);
                    }
                }
            }
        }

        internal void SetRemoteBrowser(CfrBrowser remoteBrowser, RenderProcess remoteProcess) {
            this.remoteBrowser = remoteBrowser;
            this.remoteProcess = remoteProcess;
            remoteProcess.OnExit += new Action<RenderProcess>(remoteProcess_OnExit);
        }

        void remoteProcess_OnExit(RenderProcess process) {
            if(process == this.remoteProcess) {
                this.remoteBrowser = null;
                this.remoteProcess = null;
            }
        }


        //protected override void WndProc(ref Message m) {
        //    base.WndProc(ref m);
        //    System.Diagnostics.Debug.Print(m.ToString());
        //}   
     



        protected override void OnGotFocus(System.EventArgs e) {
            base.OnGotFocus(e);
            if(BrowserHost != null) BrowserHost.SetFocus(true);
        }

        
        protected override void OnResize(System.EventArgs e) {
            base.OnResize(e);
            if(browserWindowHandle != IntPtr.Zero && this.Height > 0 && this.Width > 0) {
                SetWindowPos(browserWindowHandle, IntPtr.Zero, 0, 0, Width, Height, SWP_NOMOVE | SWP_NOZORDER);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = false)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private const uint SWP_NOMOVE = 0x2;
        private const uint SWP_NOZORDER = 0x4;
    }
}
