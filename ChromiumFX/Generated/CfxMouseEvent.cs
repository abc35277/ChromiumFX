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

// Generated file. Do not edit.


using System;

namespace Chromium {
    /// <summary>
    /// Structure representing mouse event information.
    /// </summary>
    /// <remarks>
    /// See also the original CEF documentation in
    /// <see href="https://bitbucket.org/chromiumfx/chromiumfx/src/tip/cef/include/internal/cef_types.h">cef/include/internal/cef_types.h</see>.
    /// </remarks>
    public sealed class CfxMouseEvent : CfxStructure {

        static CfxMouseEvent () {
            CfxApi.cfx_mouse_event_ctor = (CfxApi.cfx_ctor_delegate)CfxApi.GetDelegate(570, typeof(CfxApi.cfx_ctor_delegate));
            CfxApi.cfx_mouse_event_dtor = (CfxApi.cfx_dtor_delegate)CfxApi.GetDelegate(571, typeof(CfxApi.cfx_dtor_delegate));
            CfxApi.cfx_mouse_event_set_x = (CfxApi.cfx_mouse_event_set_x_delegate)CfxApi.GetDelegate(572, typeof(CfxApi.cfx_mouse_event_set_x_delegate));
            CfxApi.cfx_mouse_event_get_x = (CfxApi.cfx_mouse_event_get_x_delegate)CfxApi.GetDelegate(573, typeof(CfxApi.cfx_mouse_event_get_x_delegate));
            CfxApi.cfx_mouse_event_set_y = (CfxApi.cfx_mouse_event_set_y_delegate)CfxApi.GetDelegate(574, typeof(CfxApi.cfx_mouse_event_set_y_delegate));
            CfxApi.cfx_mouse_event_get_y = (CfxApi.cfx_mouse_event_get_y_delegate)CfxApi.GetDelegate(575, typeof(CfxApi.cfx_mouse_event_get_y_delegate));
            CfxApi.cfx_mouse_event_set_modifiers = (CfxApi.cfx_mouse_event_set_modifiers_delegate)CfxApi.GetDelegate(576, typeof(CfxApi.cfx_mouse_event_set_modifiers_delegate));
            CfxApi.cfx_mouse_event_get_modifiers = (CfxApi.cfx_mouse_event_get_modifiers_delegate)CfxApi.GetDelegate(577, typeof(CfxApi.cfx_mouse_event_get_modifiers_delegate));
        }

        public CfxMouseEvent() : base(CfxApi.cfx_mouse_event_ctor, CfxApi.cfx_mouse_event_dtor) {}

        /// <summary>
        /// X coordinate relative to the left side of the view.
        /// </summary>
        /// <remarks>
        /// See also the original CEF documentation in
        /// <see href="https://bitbucket.org/chromiumfx/chromiumfx/src/tip/cef/include/internal/cef_types.h">cef/include/internal/cef_types.h</see>.
        /// </remarks>
        public int X {
            get {
                int value;
                CfxApi.cfx_mouse_event_get_x(nativePtrUnchecked, out value);
                return value;
            }
            set {
                CfxApi.cfx_mouse_event_set_x(nativePtrUnchecked, value);
            }
        }

        /// <summary>
        /// Y coordinate relative to the top side of the view.
        /// </summary>
        /// <remarks>
        /// See also the original CEF documentation in
        /// <see href="https://bitbucket.org/chromiumfx/chromiumfx/src/tip/cef/include/internal/cef_types.h">cef/include/internal/cef_types.h</see>.
        /// </remarks>
        public int Y {
            get {
                int value;
                CfxApi.cfx_mouse_event_get_y(nativePtrUnchecked, out value);
                return value;
            }
            set {
                CfxApi.cfx_mouse_event_set_y(nativePtrUnchecked, value);
            }
        }

        /// <summary>
        /// Bit flags describing any pressed modifier keys. See
        /// CfxEventFlags for values.
        /// </summary>
        /// <remarks>
        /// See also the original CEF documentation in
        /// <see href="https://bitbucket.org/chromiumfx/chromiumfx/src/tip/cef/include/internal/cef_types.h">cef/include/internal/cef_types.h</see>.
        /// </remarks>
        public uint Modifiers {
            get {
                uint value;
                CfxApi.cfx_mouse_event_get_modifiers(nativePtrUnchecked, out value);
                return value;
            }
            set {
                CfxApi.cfx_mouse_event_set_modifiers(nativePtrUnchecked, value);
            }
        }

    }
}
