import { useThemeColors } from "@app-providers";
import { createUseStyles } from "react-jss";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    container: {
      display: "flex",
      flexDirection: "column",
      justifyContent: "flex-start",
      padding: 24,
      paddingTop: 48,
      "@media (max-width: 700px)": { padding: 12, paddingTop: 24 },
    },

    titleContainer: { marginBottom: 16 },
    title: { fontWeight: 700 },

    formCard: {
      border: `1px solid ${color.divider}`,
      borderRadius: 24,
      padding: 16,
      backgroundColor: color.paper,
      color: color.text,
      maxWidth: 600,
      marginLeft: "auto",
      marginRight: "auto",
      width: "100%",
      "@media (min-width: 600px)": { padding: 24 },
      "@media (max-width: 700px)": { width: "80%" },
    },

    formHeader: { marginBottom: 24 },

    fieldContainer: {
      marginBottom: 16,
      "@media (min-width: 600px)": { marginBottom: 24 },
    },

    fieldLabel: { marginBottom: 2.4, fontWeight: 300 },
    fieldLabelRTL: { marginBottom: 2.4, fontWeight: 300, textAlign: "right" },
    fieldLabelLTR: { marginBottom: 2.4, fontWeight: 300, textAlign: "left" },

    emailFieldLabel: { marginBottom: 2.4, fontWeight: 100 },
    emailFieldLabelRTL: {
      marginBottom: 2.4,
      fontWeight: 100,
      textAlign: "right",
    },
    emailFieldLabelLTR: {
      marginBottom: 2.4,
      fontWeight: 100,
      textAlign: "left",
    },

    textField: {
      width: "100%",
      "& .MuiInputLabel-root": { display: "none" },
      "& .MuiOutlinedInput-root": {
        backgroundColor: "transparent",
      },
      "& .MuiInputBase-input.Mui-disabled": {
        color: "var(--mui-palette-text-disabled)",
        WebkitTextFillColor: "var(--mui-palette-text-disabled)", // Safari
      },
      "& .MuiInputBase-input.Mui-error": {
        color: "var(--mui-palette-error-main)",
      },
    },

    textFieldRTL: {
      composes: "$textField",
      "& .MuiInputBase-root": { direction: "rtl" },
    },
    textFieldLTR: {
      composes: "$textField",
      "& .MuiInputBase-root": { direction: "ltr" },
    },

    emailDisabledNote: {
      marginTop: 4,
      fontWeight: 100,
      fontSize: "0.75rem",
      color: "var(--mui-palette-text-disabled)",
    },
    emailDisabledNoteRTL: {
      composes: "$emailDisabledNote",
      textAlign: "right",
    },
    emailDisabledNoteLTR: {
      composes: "$emailDisabledNote",
      textAlign: "left",
    },

    buttonContainer: {
      marginTop: 24,
      display: "flex",
      gap: 8,
      flexWrap: "wrap",
      "@media (max-width: 600px)": { flexDirection: "column", gap: 12 },
    },

    interestsContainer: {
      display: "flex",
      gap: 8,
      flexWrap: "wrap",
      marginTop: 8,
    },

    dropdownRTL: {
      "& .MuiInputBase-root": { direction: "rtl" },
      "& .MuiSelect-select, & .MuiInputBase-input": { textAlign: "right" },
    },
    dropdownLTR: {
      "& .MuiInputBase-root": { direction: "ltr" },
      "& .MuiSelect-select, & .MuiInputBase-input": { textAlign: "left" },
    },
  })();
};
