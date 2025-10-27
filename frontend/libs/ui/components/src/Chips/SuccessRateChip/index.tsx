import { Chip } from "@mui/material";

interface SuccessRateChipProps {
  value: number; // 0–100
}

export const SuccessRateChip = ({ value }: SuccessRateChipProps) => {
  const getColor = () => {
    if (value >= 80) return "#4caf50";
    if (value >= 50) return "#FDA13EFF";
    return "#f44336";
  };

  return (
    <Chip
      size="small"
      label={`${value}%`}
      sx={{
        fontWeight: 700,
        fontVariantNumeric: "tabular-nums",
        borderRadius: 999,
        border: "1px solid #c7d2fe",
        background: "#ede9fe",
        color: getColor(),
        px: 1.5,
        "& .MuiChip-label": {
          px: 1,
        },
      }}
    />
  );
};
