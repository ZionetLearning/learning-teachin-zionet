import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    listItem: {
      padding: "0.75rem 0.9rem 0.8rem",
      border: `1px solid ${color.divider}`,
      borderRadius: 10,
      display: "flex",
      alignItems: "center",
      gap: "0.85rem",
      background: color.paper,
      boxShadow: "0 1px 2px rgba(0,0,0,0.05)",
      transition:
        "box-shadow .22s ease, transform .22s ease, border-color .25s",
      "&:hover": {
        boxShadow: "0 4px 10px rgba(0,0,0,0.06)",
        transform: "translateY(-2px)",
        borderColor: color.divider,
      },
    },
    avatar: {
      width: 36,
      height: 36,
      borderRadius: "50%",
      background: `linear-gradient(135deg, rgba(var(${color.primaryMainChannel}) / 1), rgba(var(${color.primaryMainChannel}) / 0.7))`,
      color: color.primaryContrast,
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      fontWeight: 600,
      fontSize: 14,
      letterSpacing: 0.5,
      userSelect: "none",
      boxShadow: "0 2px 4px rgba(0,0,0,0.12)",
    },
    info: {
      flex: 1,
      display: "flex",
      justifyContent: "space-between",
      minWidth: 0,
      "& span": {
        fontSize: 14,
        fontWeight: 500,
        color: color.text,
      },
    },
    btnBase: {
      border: "none",
      borderRadius: 8,
      padding: "0.45rem 0.75rem 0.5rem",
      fontSize: 12,
      fontWeight: 600,
      letterSpacing: 0.4,
      cursor: "pointer",
      transition: "background .25s, transform .2s, box-shadow .25s",
      display: "inline-flex",
      alignItems: "center",
      gap: 4,
      "&:active:not(:disabled)": { transform: "translateY(1px)" },
      "&:disabled": { opacity: 0.55, cursor: "not-allowed" },
      "@media (max-width: 480px)": {
        padding: "0.3rem 0.5rem 0.35rem",
        fontSize: 10,
        minWidth: "auto",
      },
    },
    updateButton: {
      composes: "$btnBase",
      background: "linear-gradient(90deg,#3b82f6,#2563eb)",
      color: "#fff",
      "&:hover:not(:disabled)": {
        background: "linear-gradient(90deg,#2563eb,#1d4ed8)",
      },
    },
    deleteButton: {
      composes: "$btnBase",
      background: "linear-gradient(90deg,#ef4444,#dc2626)",
      color: "#fff",
      "&:hover:not(:disabled)": {
        background: "linear-gradient(90deg,#dc2626,#b91c1c)",
      },
    },
    editForm: {
      flex: 1,
      display: "flex",
      alignItems: "center",
      gap: 8,
      flexWrap: "wrap",
    },
    editInput: {
      flex: "1 1 160px",
      minWidth: 140,
      padding: "0.45rem 0.55rem",
      border: `1px solid ${color.divider}`,
      borderRadius: 8,
      fontSize: 13,
      background: `rgba(var(${color.primaryMainChannel}) / 0.03)`,
      transition: "border-color .2s, box-shadow .2s, background .3s",
      color: color.text,
      "&:focus": {
        outline: "none",
        borderColor: color.primary,
        boxShadow: `0 0 0 3px rgba(var(${color.primaryMainChannel}) / 0.25)`,
        background: color.paper,
      },
      "&:-webkit-autofill, &:-webkit-autofill:hover, &:-webkit-autofill:focus":
        {
          WebkitBoxShadow: `0 0 0 1000px ${color.paper} inset`,
          boxShadow: `0 0 0 1000px ${color.paper} inset`,
          WebkitTextFillColor: color.text,
          transition: "background-color 9999s ease-out, color 9999s ease-out",
          caretColor: color.text,
        },
    },
    editActions: {
      display: "flex",
      gap: 6,
      justifyContent: "center",
      "@media (max-width: 480px)": {
        gap: 4,
        flexDirection: "column",
      },
    },
    actions: {
      display: "flex",
      alignItems: "center",
      gap: 6,
      justifyContent: "center",
      "@media (max-width: 480px)": {
        gap: 4,
        flexDirection: "column",
      },
    },
    saveButton: {
      composes: "$btnBase",
      background: "linear-gradient(90deg,#10b981,#059669)",
      color: "#fff",
      "&:hover:not(:disabled)": {
        background: "linear-gradient(90deg,#059669,#047857)",
      },
    },
    cancelButton: {
      composes: "$btnBase",
      background: color.bg,
      color: color.text,
      border: `1px solid ${color.divider}`,
      "&:hover:not(:disabled)": {
        background: `rgba(var(${color.primaryMainChannel}) / 0.08)`,
      },
    },
    tableRow: {
      background: color.paper,
      "@media (max-width: 480px)": {
        minHeight: 48,
      },
    },
    tableCell: {
      color: color.text,
      fontSize: 14,
      "& span": { color: color.text },
      textAlign: "center",
      padding: "4px 8px",
      "@media (max-width: 768px)": {
        fontSize: 13,
        padding: "3px 6px",
      },
      "@media (max-width: 480px)": {
        fontSize: 12,
        padding: "2px 4px",
      },
    },
    dropdownCell: {
      "& label, & .MuiSelect-select, & .MuiInputBase-input": {
        color: color.text,
      },
    },
    textField: {
      "& .MuiOutlinedInput-root": {
        height: 32,
        borderRadius: 8,
        background: `rgba(var(${color.primaryMainChannel}) / 0.05)`,
        "@media (max-width: 480px)": {
          height: 28,
        },
      },
      "& .MuiOutlinedInput-input": {
        padding: "4px 8px",
        fontSize: 13,
        lineHeight: 1.2,
        backgroundColor: "transparent",
        color: color.text,
        "@media (max-width: 480px)": {
          padding: "2px 6px",
          fontSize: 11,
        },
      },
    },
    email: {
      overflow: "hidden",
      textOverflow: "ellipsis",
      whiteSpace: "nowrap",
      display: "inline-block",
      maxWidth: 200,
      color: color.text,
      "@media (max-width: 768px)": {
        maxWidth: 150,
      },
      "@media (max-width: 480px)": {
        maxWidth: 100,
      },
    },
  })();
};
