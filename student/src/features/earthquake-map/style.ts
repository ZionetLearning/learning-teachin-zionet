import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  dropdownTitle: {
    fontSize: "16px",
    fontWeight: "500",
    color: "#7c4dff",
  },
  dropdownWrapper: {
    padding: "10px",
    display: "flex",
    gap: "10px",
    flexDirection: "row",
    alignItems: "center",
  },
  formControl: {
    "& .MuiOutlinedInput-root": {
      "& fieldset": {
        borderColor: "#7c4dff",
      },
      "&:hover fieldset": {
        borderColor: "#7c4dff",
      },
      "&.Mui-focused fieldset": {
        borderColor: "#7c4dff",
      },
    },
    "& .MuiSelect-outlined": {
      color: "#7c4dff",
    },
    "& .MuiSvgIcon-root": {
      color: "#7c4dff",
    },
  },
});
