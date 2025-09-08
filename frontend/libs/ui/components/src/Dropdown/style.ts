import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  wrapper: {
    display: "flex",
    flexDirection: "column",
    fontSize: 14,
    color: "#2d3748",
    fontWeight: 500,

    // Label
    "&& .MuiInputLabel-root": {
      fontSize: 14,
      color: "#2d3748",
    },

    // Root of the input (OutlinedInput under the Select)
    "&& .MuiOutlinedInput-root": {
      background: "#f8fafc",
      borderRadius: 8,
      transition: "border-color .2s, box-shadow .2s, background .3s",

      "& .MuiOutlinedInput-notchedOutline": {
        borderColor: "#cbd5e0",
      },

      "&:hover .MuiOutlinedInput-notchedOutline": {
        borderColor: "#a0aec0",
      },

      "&.Mui-focused": {
        background: "#fff",
        boxShadow: "0 0 0 3px rgba(99,102,241,0.25)",
      },
      "&.Mui-focused .MuiOutlinedInput-notchedOutline": {
        borderColor: "#6366f1",
      },

      "&.Mui-disabled": {
        background: "#f1f5f9",
        color: "#94a3b8",
      },

      // The clickable "input" area inside select
      "& .MuiSelect-select": {
        marginTop: 4,
        padding: "0.55rem 0.7rem",
        marginRight: 24,
        fontSize: 14,
        color: "#1a202c",
      },

      // Placeholder
      "& .MuiSelect-select.MuiPlaceholder": {
        color: "#94a3b8",
      },
    },
  },
});
