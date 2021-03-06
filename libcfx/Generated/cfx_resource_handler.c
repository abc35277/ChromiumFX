// Copyright (c) 2014-2017 Wolfgang Borgsmüller
// All rights reserved.
// 
// This software may be modified and distributed under the terms
// of the BSD license. See the License.txt file for details.

// Generated file. Do not edit.


// cef_resource_handler

typedef struct _cfx_resource_handler_t {
    cef_resource_handler_t cef_resource_handler;
    unsigned int ref_count;
    gc_handle_t gc_handle;
    int wrapper_kind;
    // managed callbacks
    void (CEF_CALLBACK *process_request)(gc_handle_t self, int* __retval, cef_request_t* request, int *request_release, cef_callback_t* callback, int *callback_release);
    void (CEF_CALLBACK *get_response_headers)(gc_handle_t self, cef_response_t* response, int *response_release, int64* response_length, char16 **redirectUrl_str, int *redirectUrl_length, gc_handle_t *redirectUrl_gc_handle);
    void (CEF_CALLBACK *read_response)(gc_handle_t self, int* __retval, void* data_out, int bytes_to_read, int* bytes_read, cef_callback_t* callback, int *callback_release);
    void (CEF_CALLBACK *can_get_cookie)(gc_handle_t self, int* __retval, const cef_cookie_t* cookie);
    void (CEF_CALLBACK *can_set_cookie)(gc_handle_t self, int* __retval, const cef_cookie_t* cookie);
    void (CEF_CALLBACK *cancel)(gc_handle_t self);
} cfx_resource_handler_t;

void CEF_CALLBACK _cfx_resource_handler_add_ref(struct _cef_base_ref_counted_t* base) {
    InterlockedIncrement(&((cfx_resource_handler_t*)base)->ref_count);
}
int CEF_CALLBACK _cfx_resource_handler_release(struct _cef_base_ref_counted_t* base) {
    int count = InterlockedDecrement(&((cfx_resource_handler_t*)base)->ref_count);
    if(count == 0) {
        if(((cfx_resource_handler_t*)base)->wrapper_kind == 0) {
            cfx_gc_handle_switch(&((cfx_resource_handler_t*)base)->gc_handle, GC_HANDLE_FREE);
        } else {
            cfx_gc_handle_switch(&((cfx_resource_handler_t*)base)->gc_handle, GC_HANDLE_FREE | GC_HANDLE_REMOTE);
        }
        free(base);
        return 1;
    }
    return 0;
}
int CEF_CALLBACK _cfx_resource_handler_has_one_ref(struct _cef_base_ref_counted_t* base) {
    return ((cfx_resource_handler_t*)base)->ref_count == 1 ? 1 : 0;
}

static cfx_resource_handler_t* cfx_resource_handler_ctor(gc_handle_t gc_handle, int wrapper_kind) {
    cfx_resource_handler_t* ptr = (cfx_resource_handler_t*)calloc(1, sizeof(cfx_resource_handler_t));
    if(!ptr) return 0;
    ptr->cef_resource_handler.base.size = sizeof(cef_resource_handler_t);
    ptr->cef_resource_handler.base.add_ref = _cfx_resource_handler_add_ref;
    ptr->cef_resource_handler.base.release = _cfx_resource_handler_release;
    ptr->cef_resource_handler.base.has_one_ref = _cfx_resource_handler_has_one_ref;
    ptr->ref_count = 1;
    ptr->gc_handle = gc_handle;
    ptr->wrapper_kind = wrapper_kind;
    return ptr;
}

// process_request

int CEF_CALLBACK cfx_resource_handler_process_request(cef_resource_handler_t* self, cef_request_t* request, cef_callback_t* callback) {
    int __retval;
    int request_release;
    int callback_release;
    ((cfx_resource_handler_t*)self)->process_request(((cfx_resource_handler_t*)self)->gc_handle, &__retval, request, &request_release, callback, &callback_release);
    if(request_release && request) request->base.release((cef_base_ref_counted_t*)request);
    if(callback_release && callback) callback->base.release((cef_base_ref_counted_t*)callback);
    return __retval;
}

// get_response_headers

void CEF_CALLBACK cfx_resource_handler_get_response_headers(cef_resource_handler_t* self, cef_response_t* response, int64* response_length, cef_string_t* redirectUrl) {
    int response_release;
    char16* redirectUrl_tmp_str = 0; int redirectUrl_tmp_length = 0; gc_handle_t redirectUrl_gc_handle = 0;
    ((cfx_resource_handler_t*)self)->get_response_headers(((cfx_resource_handler_t*)self)->gc_handle, response, &response_release, response_length, &redirectUrl_tmp_str, &redirectUrl_tmp_length, &redirectUrl_gc_handle);
    if(response_release && response) response->base.release((cef_base_ref_counted_t*)response);
    if(redirectUrl_tmp_length > 0) {
        cef_string_set(redirectUrl_tmp_str, redirectUrl_tmp_length, redirectUrl, 1);
        cfx_gc_handle_switch(&redirectUrl_gc_handle, GC_HANDLE_FREE);
    }
}

// read_response

int CEF_CALLBACK cfx_resource_handler_read_response(cef_resource_handler_t* self, void* data_out, int bytes_to_read, int* bytes_read, cef_callback_t* callback) {
    int __retval;
    int callback_release;
    ((cfx_resource_handler_t*)self)->read_response(((cfx_resource_handler_t*)self)->gc_handle, &__retval, data_out, bytes_to_read, bytes_read, callback, &callback_release);
    if(callback_release && callback) callback->base.release((cef_base_ref_counted_t*)callback);
    return __retval;
}

// can_get_cookie

int CEF_CALLBACK cfx_resource_handler_can_get_cookie(cef_resource_handler_t* self, const cef_cookie_t* cookie) {
    int __retval;
    ((cfx_resource_handler_t*)self)->can_get_cookie(((cfx_resource_handler_t*)self)->gc_handle, &__retval, cookie);
    return __retval;
}

// can_set_cookie

int CEF_CALLBACK cfx_resource_handler_can_set_cookie(cef_resource_handler_t* self, const cef_cookie_t* cookie) {
    int __retval;
    ((cfx_resource_handler_t*)self)->can_set_cookie(((cfx_resource_handler_t*)self)->gc_handle, &__retval, cookie);
    return __retval;
}

// cancel

void CEF_CALLBACK cfx_resource_handler_cancel(cef_resource_handler_t* self) {
    ((cfx_resource_handler_t*)self)->cancel(((cfx_resource_handler_t*)self)->gc_handle);
}

static void cfx_resource_handler_set_callback(cef_resource_handler_t* self, int index, void* callback) {
    switch(index) {
    case 0:
        ((cfx_resource_handler_t*)self)->process_request = (void (CEF_CALLBACK *)(gc_handle_t self, int* __retval, cef_request_t* request, int *request_release, cef_callback_t* callback, int *callback_release))callback;
        self->process_request = callback ? cfx_resource_handler_process_request : 0;
        break;
    case 1:
        ((cfx_resource_handler_t*)self)->get_response_headers = (void (CEF_CALLBACK *)(gc_handle_t self, cef_response_t* response, int *response_release, int64* response_length, char16 **redirectUrl_str, int *redirectUrl_length, gc_handle_t *redirectUrl_gc_handle))callback;
        self->get_response_headers = callback ? cfx_resource_handler_get_response_headers : 0;
        break;
    case 2:
        ((cfx_resource_handler_t*)self)->read_response = (void (CEF_CALLBACK *)(gc_handle_t self, int* __retval, void* data_out, int bytes_to_read, int* bytes_read, cef_callback_t* callback, int *callback_release))callback;
        self->read_response = callback ? cfx_resource_handler_read_response : 0;
        break;
    case 3:
        ((cfx_resource_handler_t*)self)->can_get_cookie = (void (CEF_CALLBACK *)(gc_handle_t self, int* __retval, const cef_cookie_t* cookie))callback;
        self->can_get_cookie = callback ? cfx_resource_handler_can_get_cookie : 0;
        break;
    case 4:
        ((cfx_resource_handler_t*)self)->can_set_cookie = (void (CEF_CALLBACK *)(gc_handle_t self, int* __retval, const cef_cookie_t* cookie))callback;
        self->can_set_cookie = callback ? cfx_resource_handler_can_set_cookie : 0;
        break;
    case 5:
        ((cfx_resource_handler_t*)self)->cancel = (void (CEF_CALLBACK *)(gc_handle_t self))callback;
        self->cancel = callback ? cfx_resource_handler_cancel : 0;
        break;
    }
}

