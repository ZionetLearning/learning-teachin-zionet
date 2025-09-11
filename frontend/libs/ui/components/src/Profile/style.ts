import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    minHeight: "100dvh",
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    padding: 16,
    "@media (min-width: 600px)": {
      paddingLeft: 32,
      paddingRight: 32,
    },
    "@media (min-width: 900px)": {
      paddingLeft: 64,
      paddingRight: 64,
    },
    "@media (min-width: 1200px)": {
      paddingLeft: 320,
      paddingRight: 320,
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
    padding: 24,
    backgroundColor: "#ffffff",
    maxWidth: 600,
    marginLeft: "auto",
    marginRight: "auto",
    width: "100%",
  },

  formHeader: {
    marginBottom: 24,
  },

  fieldContainer: {
    marginBottom: 24,
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
  },
});
