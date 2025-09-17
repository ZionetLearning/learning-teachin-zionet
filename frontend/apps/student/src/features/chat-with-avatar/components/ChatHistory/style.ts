import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  sidebarToggle: {
    position: "absolute",
    top: "16px",
    left: "16px",
    zIndex: 1000,
    background: "rgba(255, 255, 255, 0.95)",
    backdropFilter: "blur(8px)",
    color: "#374151",
    border: "1px solid rgba(209, 213, 219, 0.5)",
    borderRadius: "12px",
    padding: "12px",
    cursor: "pointer",
    fontSize: "16px",
    boxShadow: "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)",
    transition: "all 0.2s ease",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    "[dir='rtl'] &": {
      left: "auto",
      right: "16px",
    },
    "&:hover": {
      background: "rgba(249, 250, 251, 0.95)",
      transform: "translateY(-1px)",
      boxShadow: "0 8px 25px -5px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)",
    },
    "&:active": {
      transform: "translateY(0)",
    },
  },

  hamburgerIcon: {
    display: "flex",
    flexDirection: "column",
    width: "20px",
    height: "16px",
    "& span": {
      display: "block",
      height: "2px",
      width: "100%",
      backgroundColor: "currentColor",
      borderRadius: "1px",
      opacity: 1,
      transform: "rotate(0deg)",
      transition: "0.2s ease-in-out",
      "&:nth-child(1)": {
        transformOrigin: "left center",
      },
      "&:nth-child(2)": {
        transformOrigin: "left center",
        margin: "4px 0",
      },
      "&:nth-child(3)": {
        transformOrigin: "left center",
      },
    },
  },

  // Sidebar Styles
  sidebar: {
    width: "320px",
    backgroundColor: "#ffffff",
    borderRight: "1px solid #e5e7eb",
    display: "flex",
    flexDirection: "column",
    transform: "translateX(-100%)",
    transition: "transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)",
    position: "absolute",
    height: "100%",
    zIndex: 999,
    left: 0,
    top: 0,
    boxShadow: "0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)",
    "[dir='rtl'] &": {
      borderRight: "none",
      borderLeft: "1px solid #e5e7eb",
      left: "auto",
      right: 0,
      transform: "translateX(100%)",
    },
    "@media (max-width: 768px)": {
      width: "90%",
      maxWidth: "320px",
    },
  },
  
  sidebarOpen: {
    transform: "translateX(0)",
    "[dir='rtl'] &": {
      transform: "translateX(0)",
    },
  },

  sidebarHeader: {
    padding: "24px 20px 20px",
    borderBottom: "1px solid #f3f4f6",
    backgroundColor: "#fafafa",
    background: "linear-gradient(135deg, #fafafa 0%, #f8f9fa 100%)",
  },

  headerTop: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    marginBottom: "16px",
  },

  title: {
    margin: 0,
    fontSize: "20px",
    fontWeight: "700",
    color: "#111827",
    letterSpacing: "-0.025em",
  },

  closeButton: {
    background: "none",
    border: "none",
    fontSize: "24px",
    cursor: "pointer",
    color: "#6b7280",
    padding: "4px 8px",
    borderRadius: "6px",
    transition: "all 0.2s ease",
    "&:hover": {
      backgroundColor: "#f3f4f6",
      color: "#374151",
    },
  },

  newChatButton: {
    width: "100%",
    padding: "12px 16px",
    backgroundColor: "#3b82f6",
    background: "linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)",
    color: "white",
    border: "none",
    borderRadius: "12px",
    cursor: "pointer",
    fontSize: "14px",
    fontWeight: "600",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    gap: "8px",
    transition: "all 0.2s ease",
    boxShadow: "0 4px 6px -1px rgba(59, 130, 246, 0.2)",
    "&:hover": {
      backgroundColor: "#2563eb",
      transform: "translateY(-1px)",
      boxShadow: "0 8px 25px -5px rgba(59, 130, 246, 0.3)",
    },
    "&:active": {
      transform: "translateY(0)",
    },
  },

  plusIcon: {
    fontSize: "18px",
    fontWeight: "bold",
    lineHeight: 1,
  },

  chatList: {
    flex: 1,
    overflowY: "auto",
    padding: "8px 0",
    "&::-webkit-scrollbar": {
      width: "4px",
    },
    "&::-webkit-scrollbar-track": {
      backgroundColor: "transparent",
    },
    "&::-webkit-scrollbar-thumb": {
      backgroundColor: "#d1d5db",
      borderRadius: "2px",
      "&:hover": {
        backgroundColor: "#9ca3af",
      },
    },
  },

  chatItems: {
    display: "flex",
    flexDirection: "column",
    gap: "2px",
    padding: "0 8px",
  },

  chatItem: {
    padding: "14px 16px",
    cursor: "pointer",
    borderRadius: "12px",
    transition: "all 0.2s cubic-bezier(0.4, 0, 0.2, 1)",
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    border: "1px solid transparent",
    "&:hover": {
      backgroundColor: "#f8fafc",
      borderColor: "#e2e8f0",
      transform: "translateX(4px)",
      "[dir='rtl'] &": {
        transform: "translateX(-4px)",
      },
    },
    "&:focus": {
      outline: "2px solid #3b82f6",
      outlineOffset: "2px",
    },
  },

  activeChatItem: {
    backgroundColor: "#eff6ff",
    borderColor: "#3b82f6",
    boxShadow: "0 4px 6px -1px rgba(59, 130, 246, 0.1)",
    "&:hover": {
      backgroundColor: "#dbeafe",
      transform: "translateX(2px)",
      "[dir='rtl'] &": {
        transform: "translateX(-2px)",
      },
    },
  },

  chatContent: {
    flex: 1,
    minWidth: 0,
  },

  chatName: {
    fontSize: "15px",
    fontWeight: "600",
    color: "#1f2937",
    marginBottom: "4px",
    lineHeight: "1.4",
    wordBreak: "break-word",
  },

  chatDate: {
    fontSize: "12px",
    color: "#6b7280",
    fontWeight: "500",
  },

  chatIcon: {
    marginLeft: "12px",
    color: "#9ca3af",
    flexShrink: 0,
    transition: "color 0.2s ease",
    "[dir='rtl'] &": {
      marginLeft: 0,
      marginRight: "12px",
    },
    "$chatItem:hover &": {
      color: "#6b7280",
    },
    "$activeChatItem &": {
      color: "#3b82f6",
    },
  },

  // Loading States
  loadingContainer: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    padding: "40px 20px",
    gap: "16px",
  },

  loadingSpinner: {
    width: "32px",
    height: "32px",
    border: "3px solid #f3f4f6",
    borderTop: "3px solid #3b82f6",
    borderRadius: "50%",
    animation: "$spin 1s linear infinite",
  },

  loadingText: {
    color: "#6b7280",
    fontSize: "14px",
    fontWeight: "500",
  },

  loadingHistory: {
    padding: "12px 20px",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    gap: "8px",
    fontSize: "12px",
    color: "#6b7280",
    backgroundColor: "#fef3c7",
    borderTop: "1px solid #fde68a",
    fontWeight: "500",
  },

  loadingSpinnerSmall: {
    width: "16px",
    height: "16px",
    border: "2px solid #f3f4f6",
    borderTop: "2px solid #d97706",
    borderRadius: "50%",
    animation: "$spin 1s linear infinite",
  },

  // Empty State
  emptyState: {
    padding: "60px 20px",
    textAlign: "center",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    gap: "16px",
  },

  emptyIcon: {
    color: "#d1d5db",
    opacity: 0.8,
  },

  emptyTitle: {
    fontSize: "16px",
    fontWeight: "600",
    color: "#4b5563",
    marginBottom: "4px",
  },

  emptyDescription: {
    fontSize: "14px",
    color: "#9ca3af",
    lineHeight: "1.5",
    maxWidth: "240px",
  },

  // Sidebar Overlay for mobile
  sidebarOverlay: {
    position: "fixed",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: "rgba(0, 0, 0, 0.4)",
    backdropFilter: "blur(2px)",
    zIndex: 998,
    opacity: 1,
    animation: "$fadeIn 0.3s ease",
    "@media (min-width: 769px)": {
      display: "none",
    },
  },

  // Animations
  "@keyframes spin": {
    "0%": { transform: "rotate(0deg)" },
    "100%": { transform: "rotate(360deg)" },
  },

  "@keyframes fadeIn": {
    "0%": { opacity: 0 },
    "100%": { opacity: 1 },
  },
});
