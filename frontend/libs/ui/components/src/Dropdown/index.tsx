import {
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  SelectChangeEvent,
  FormHelperText,
} from "@mui/material";
import { useStyles } from "./style";

type Option = {
  value: string;
  label: string;
};

interface DropdownProps {
  name?: string;
  label?: string;
  options: Option[];
  value: string;
  onChange: (value: string) => void;
  error?: boolean;
  helperText?: string;
  disabled?: boolean;
  "data-testid"?: string;
}

export function Dropdown({
  name,
  label,
  options,
  value,
  onChange,
  error,
  helperText,
  disabled,
  ...rest
}: DropdownProps) {
  const classes = useStyles();

  const labelId = name ? `${name}-label` : undefined;

  const handleChange = (event: SelectChangeEvent<string>) => {
    onChange(event.target.value);
  };

  return (
    <FormControl
      fullWidth
      error={!!error}
      disabled={disabled}
      className={classes.wrapper}
    >
      <InputLabel id={labelId}>{label}</InputLabel>
      <Select
        labelId={labelId}
        id={name}
        name={name}
        value={value ?? ""}
        onChange={handleChange}
        label={label}
        {...rest}
      >
        {options.map((opt) => (
          <MenuItem key={opt.value} value={opt.value}>
            {opt.label}
          </MenuItem>
        ))}
      </Select>
      {helperText ? <FormHelperText>{helperText}</FormHelperText> : null}
    </FormControl>
  );
}
