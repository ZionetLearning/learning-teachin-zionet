import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    cardBase: {
      background: color.paper,
      border: `1px solid ${color.divider}`,
      boxShadow: "0 2px 4px rgba(0,0,0,0.04), 0 4px 12px rgba(0,0,0,0.03)",
      borderRadius: 12,
      padding: "1.25rem 1.15rem 1.5rem",
      position: "relative",
      transition: "box-shadow .25s ease, transform .25s ease",
      "&:hover": {
        boxShadow: "0 4px 10px rgba(0,0,0,0.06), 0 6px 18px rgba(0,0,0,0.05)",
        transform: "translateY(-2px)",
      },
    },
    formContainer: {
      composes: "$cardBase",
      flex: "1 1 40%",
      maxWidth: 430,
      alignSelf: "center",
      "@media (max-width: 768px)": {
        flex: "0 0 auto",
        maxWidth: "100%",
        alignSelf: "stretch",
      },
      "@media (max-width: 480px)": {
        padding: "1rem 0.75rem 1.25rem",
      },
    },
    sectionTitle: {
      margin: "0 0 0.85rem",
      fontSize: 18,
      fontWeight: 600,
      letterSpacing: 0.3,
      color: color.text,
      textAlign: "center",
    },
    form: {
      display: "flex",
      flexDirection: "column",
      gap: "0.85rem",
      "@media (max-width: 480px)": {
        gap: "0.75rem",
      },
    },
    label: {
      display: "flex",
      flexDirection: "column",
      fontSize: 14,
      color: color.text,
      fontWeight: 500,
      "& input": {
        marginTop: 4,
        padding: "0.55rem 0.7rem",
        border: `1px solid ${color.divider}`,
        borderRadius: 8,
        fontSize: 14,
        color: color.text,
        background: `rgba(var(${color.primaryMainChannel}) / 0.03)`,
        transition: "border-color .2s, box-shadow .2s, background .3s",
        "&:focus": {
          outline: "none",
          borderColor: color.primary,
          boxShadow: `0 0 0 3px rgba(var(${color.primaryMainChannel}) / 0.25)`,
          background: color.paper,
        },
        "&::placeholder": {
          color: color.textMuted,
          opacity: 1,
        },
        "&:-webkit-autofill, &:-webkit-autofill:hover, &:-webkit-autofill:focus":
          {
            WebkitBoxShadow: `0 0 0 1000px ${color.paper} inset`,
            boxShadow: `0 0 0 1000px ${color.paper} inset`,
            WebkitTextFillColor: color.text,
            caretColor: color.text,
            transition: "background-color 9999s ease-out, color 9999s ease-out",
          },
        "@media (max-width: 480px)": {
          padding: "0.5rem 0.6rem",
          fontSize: 13,
        },
      },
      "@media (max-width: 480px)": {
        fontSize: 13,
      },
    },
    error: {
      color: "red",
      fontSize: 11,
      marginTop: 2,
    },
    submitButton: {
      marginTop: "0.4rem",
      background: `linear-gradient(90deg, rgba(var(${color.primaryMainChannel}) / 1), rgba(var(${color.primaryMainChannel}) / 1))`,
      color: color.primaryContrast,
      fontWeight: 600,
      border: "none",
      borderRadius: 10,
      padding: "0.65rem 1rem",
      fontSize: 14,
      cursor: "pointer",
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      lineHeight: 1.15,
      textShadow: "0 1px 2px rgba(0,0,0,0.25)",
      boxShadow: "0 2px 4px rgba(0,0,0,0.15)",
      transition: "background .25s, transform .2s, box-shadow .25s",
      "&:hover:not(:disabled)": {
        background: `linear-gradient(90deg, rgba(var(${color.primaryMainChannel}) / 1), rgba(var(${color.primaryMainChannel}) / 1))`,
      },
      "&:active:not(:disabled)": {
        transform: "translateY(1px)",
      },
      "&:disabled": {
        opacity: 0.6,
        cursor: "not-allowed",
      },
    },
  })();
};
