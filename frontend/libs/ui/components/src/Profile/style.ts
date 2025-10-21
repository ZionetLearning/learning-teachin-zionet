import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    height: "100vh",
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    padding: 24,
    "@media (max-width: 700px)": {
      padding: 12,
    },
  },
  titleContainer: {
    marginBottom: 16,
  },
  title: {
    fontWeight: 700,
  },
  formCard: {
    border: "1px solid #e0e0e0",
    borderRadius: 24,
    padding: 16,
    backgroundColor: "#ffffff",
    maxWidth: 600,
    marginLeft: "auto",
    marginRight: "auto",
    width: "100%",
    "@media (min-width: 600px)": {
      padding: 24,
    },
    "@media (max-width: 700px)": {
      width: "80%",
    },
  },
  formHeader: {
    marginBottom: 24,
  },
  fieldContainer: {
    marginBottom: 16,
    "@media (min-width: 600px)": {
      marginBottom: 24,
    },
  },
  fieldLabel: {
    marginBottom: 2.4,
    fontWeight: 300,
  },
  fieldLabelRTL: {
    marginBottom: 2.4,
    fontWeight: 300,
    textAlign: "right",
  },
  fieldLabelLTR: {
    marginBottom: 2.4,
    fontWeight: 300,
    textAlign: "left",
  },
  emailFieldLabel: {
    marginBottom: 2.4,
    fontWeight: 100,
  },
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
    "& .MuiInputLabel-root": {
      display: "none",
    },
    "&.Mui-disabled": {
      color: "#9e9e9e",
    },
    "&.Mui-error": {
      color: "#f44336",
    },
  },
  textFieldRTL: {
    width: "100%",
    "& .MuiInputLabel-root": {
      display: "none",
    },
    "& .MuiInputBase-root": {
      direction: "rtl",
    },
    "&.Mui-disabled": {
      color: "#9e9e9e",
    },
    "&.Mui-error": {
      color: "#f44336",
    },
  },
  textFieldLTR: {
    width: "100%",
    "& .MuiInputLabel-root": {
      display: "none",
    },
    "& .MuiInputBase-root": {
      direction: "ltr",
    },
    "&.Mui-disabled": {
      color: "#9e9e9e",
    },
    "&.Mui-error": {
      color: "#f44336",
    },
  },
  emailDisabledNote: {
    marginTop: 4,
    fontWeight: 100,
    fontSize: "0.75rem",
    color: "#9e9e9e",
  },
  emailDisabledNoteRTL: {
    marginTop: 4,
    fontWeight: 100,
    fontSize: "0.75rem",
    color: "#9e9e9e",
    textAlign: "right",
  },
  emailDisabledNoteLTR: {
    marginTop: 4,
    fontWeight: 100,
    fontSize: "0.75rem",
    color: "#9e9e9e",
    textAlign: "left",
  },
  buttonContainer: {
    marginTop: 24,
    display: "flex",
    gap: 8,
    flexWrap: "wrap",
    "@media (max-width: 600px)": {
      flexDirection: "column",
      gap: 12,
    },
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
});
