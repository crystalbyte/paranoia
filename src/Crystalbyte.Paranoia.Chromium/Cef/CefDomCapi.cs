#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefDomvisitor {
        public CefBase Base;
        public IntPtr Visit;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefDomdocument {
        public CefBase Base;
        public IntPtr GetElementType;
        public IntPtr GetDocument;
        public IntPtr GetBody;
        public IntPtr GetHead;
        public IntPtr GetTitle;
        public IntPtr GetElementById;
        public IntPtr GetFocusedNode;
        public IntPtr HasSelection;
        public IntPtr GetSelectionStartOffset;
        public IntPtr GetSelectionEndOffset;
        public IntPtr GetSelectionAsMarkup;
        public IntPtr GetSelectionAsText;
        public IntPtr GetBaseUrl;
        public IntPtr GetCompleteUrl;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefDomnode {
        public CefBase Base;
        public IntPtr GetElementType;
        public IntPtr IsText;
        public IntPtr IsElement;
        public IntPtr IsEditable;
        public IntPtr IsFormControlElement;
        public IntPtr GetFormControlElementType;
        public IntPtr IsSame;
        public IntPtr GetName;
        public IntPtr GetValue;
        public IntPtr SetValue;
        public IntPtr GetAsMarkup;
        public IntPtr GetDocument;
        public IntPtr GetParent;
        public IntPtr GetPreviousSibling;
        public IntPtr GetNextSibling;
        public IntPtr HasChildren;
        public IntPtr GetFirstChild;
        public IntPtr GetLastChild;
        public IntPtr GetElementTagName;
        public IntPtr HasElementAttributes;
        public IntPtr HasElementAttribute;
        public IntPtr GetElementAttribute;
        public IntPtr GetElementAttributes;
        public IntPtr SetElementAttribute;
        public IntPtr GetElementInnerText;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefDomCapiDelegates {
        public delegate void VisitCallback3(IntPtr self, IntPtr document);

        public delegate CefDomDocumentType GetTypeCallback(IntPtr self);

        public delegate IntPtr GetDocumentCallback(IntPtr self);

        public delegate IntPtr GetBodyCallback(IntPtr self);

        public delegate IntPtr GetHeadCallback(IntPtr self);

        public delegate IntPtr GetTitleCallback(IntPtr self);

        public delegate IntPtr GetElementByIdCallback(IntPtr self, IntPtr id);

        public delegate IntPtr GetFocusedNodeCallback(IntPtr self);

        public delegate int HasSelectionCallback(IntPtr self);

        public delegate int GetSelectionStartOffsetCallback(IntPtr self);

        public delegate int GetSelectionEndOffsetCallback(IntPtr self);

        public delegate IntPtr GetSelectionAsMarkupCallback(IntPtr self);

        public delegate IntPtr GetSelectionAsTextCallback(IntPtr self);

        public delegate IntPtr GetBaseUrlCallback(IntPtr self);

        public delegate IntPtr GetCompleteUrlCallback(IntPtr self, IntPtr partialurl);

        public delegate CefDomNodeType GetTypeCallback2(IntPtr self);

        public delegate int IsTextCallback(IntPtr self);

        public delegate int IsElementCallback(IntPtr self);

        public delegate int IsEditableCallback2(IntPtr self);

        public delegate int IsFormControlElementCallback(IntPtr self);

        public delegate IntPtr GetFormControlElementTypeCallback(IntPtr self);

        public delegate int IsSameCallback2(IntPtr self, IntPtr that);

        public delegate IntPtr GetNameCallback(IntPtr self);

        public delegate IntPtr GetValueCallback(IntPtr self);

        public delegate int SetValueCallback(IntPtr self, IntPtr value);

        public delegate IntPtr GetAsMarkupCallback(IntPtr self);

        public delegate IntPtr GetDocumentCallback2(IntPtr self);

        public delegate IntPtr GetParentCallback(IntPtr self);

        public delegate IntPtr GetPreviousSiblingCallback(IntPtr self);

        public delegate IntPtr GetNextSiblingCallback(IntPtr self);

        public delegate int HasChildrenCallback(IntPtr self);

        public delegate IntPtr GetFirstChildCallback(IntPtr self);

        public delegate IntPtr GetLastChildCallback(IntPtr self);

        public delegate IntPtr GetElementTagNameCallback(IntPtr self);

        public delegate int HasElementAttributesCallback(IntPtr self);

        public delegate int HasElementAttributeCallback(IntPtr self, IntPtr attrname);

        public delegate IntPtr GetElementAttributeCallback(IntPtr self, IntPtr attrname);

        public delegate void GetElementAttributesCallback(IntPtr self, IntPtr attrmap);

        public delegate int SetElementAttributeCallback(IntPtr self, IntPtr attrname, IntPtr value);

        public delegate IntPtr GetElementInnerTextCallback(IntPtr self);
    }
}