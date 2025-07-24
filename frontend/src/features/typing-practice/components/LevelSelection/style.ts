import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
    levelSelection: {
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        flex: 1,
        gap: "12px",
    },
    levelTitle: { fontSize: "16px", fontWeight: "600", color: "#333333" },
    levelDescription: { fontSize: "14px", color: "#6c757d", textAlign: "center" },
    levelButtons: { display: "flex", gap: "8px", flexWrap: "wrap", justifyContent: "center" },
    levelButton: {
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        padding: "12px",
        border: "2px solid #e1e5e9",
        borderRadius: "10px",
        backgroundColor: "#ffffff",
        cursor: "pointer",
        minWidth: "140px",
        textAlign: "center",
        "&:hover": { borderColor: "#007bff", backgroundColor: "#f8f9fa" },
        "&:disabled": { opacity: 0.6, cursor: "not-allowed" },
    },
    levelButtonIcon: { fontSize: "25px", marginBottom: "8px" },
    levelButtonLabel: { fontSize: "16px", fontWeight: "600" },
    levelButtonDescription: { fontSize: "12px", color: "#6c757d" },

    // Mobile responsive
    "@media (max-width: 768px)": {
        levelButtons: { flexDirection: "column", width: "100%" },
        levelButton: { minWidth: "auto", width: "100%" },
    },
});
