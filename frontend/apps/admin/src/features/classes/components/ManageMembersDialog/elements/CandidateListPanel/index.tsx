import {
  Box,
  Button,
  Checkbox,
  IconButton,
  InputAdornment,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  MenuItem,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import AddIcon from "@mui/icons-material/PersonAddAlt";
import { useTranslation } from "react-i18next";
import { type User, type AppRoleType } from "@app-providers";
import { RoleChip } from "@ui-components";
import { useStyles } from "./style";

type StudentTeacherRole = Exclude<AppRoleType, "admin">;

type Props = {
  users: User[];
  memberIdSet: Set<string>;
  selectedIds: Set<string>;
  onToggle: (userId: string) => void;
  onSelectAll: () => void;
  onClearAll: () => void;
  onAddSingle: (userId: string) => void;
  onBatchAdd: () => void;
  pendingAdd: boolean;
  addableCount: number;
  roleFilter: StudentTeacherRole | "All";
  setRoleFilter: (r: StudentTeacherRole | "All") => void;
  query: string;
  setQuery: (s: string) => void;
  studentsCount: number;
  teachersCount: number;
};

const getFullName = (u: User) => `${u.firstName} ${u.lastName}`.trim();

export const CandidateListPanel = ({
  users,
  memberIdSet,
  selectedIds,
  onToggle,
  onSelectAll,
  onClearAll,
  onAddSingle,
  onBatchAdd,
  pendingAdd,
  addableCount,
  roleFilter,
  setRoleFilter,
  query,
  setQuery,
  studentsCount,
  teachersCount,
}: Props) => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
    <Box className={classes.container}>
      {/* Filters */}
      <Stack
        direction="row"
        gap={1}
        alignItems="center"
        className={classes.filtersRow}
      >
        <Select
          size="small"
          value={roleFilter}
          onChange={(e) =>
            setRoleFilter(e.target.value as StudentTeacherRole | "All")
          }
          className={classes.roleSelect}
        >
          <MenuItem value="All">{t("pages.classes.allRoles")}</MenuItem>
          <MenuItem value="student">
            {t("pages.classes.students")} ({studentsCount})
          </MenuItem>
          <MenuItem value="teacher">
            {t("pages.classes.teachers")} ({teachersCount})
          </MenuItem>
        </Select>

        <TextField
          size="small"
          placeholder={t("pages.classes.searchByNameEmail")}
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            },
          }}
          fullWidth
        />
      </Stack>

      {/* Toolbar */}
      <Box className={classes.toolbarRow}>
        <Button size="small" onClick={onSelectAll}>
          {t("pages.classes.selectAll")}
        </Button>
        <Button size="small" onClick={onClearAll}>
          {t("pages.classes.clear")}
        </Button>
        <Button
          size="small"
          variant="contained"
          onClick={onBatchAdd}
          disabled={pendingAdd || addableCount === 0}
        >
          {t("pages.classes.addSelected")} ({addableCount})
        </Button>
      </Box>

      {/* List */}
      <List dense className={classes.list}>
        {users.map((u) => {
          const isMember = memberIdSet.has(u.userId);
          const checked = selectedIds.has(u.userId);
          const labelId = `candidate-${u.userId}`;

          return (
            <ListItem
              key={u.userId}
              disablePadding
              className={`${classes.listItem} ${isMember ? classes.listItemDisabled : ""}`}
              secondaryAction={
                <Tooltip
                  title={
                    isMember
                      ? t("pages.classes.alreadyInClass")
                      : t("pages.classes.addToClass")
                  }
                >
                  <span>
                    <IconButton
                      edge="end"
                      onClick={(e) => {
                        e.stopPropagation();
                        if (!isMember) onAddSingle(u.userId);
                      }}
                      disabled={pendingAdd || isMember}
                    >
                      <AddIcon />
                    </IconButton>
                  </span>
                </Tooltip>
              }
            >
              <ListItemButton
                dense
                onClick={() => !isMember && onToggle(u.userId)}
                className={`${classes.listItemButton} ${checked ? classes.listItemButtonSelected : ""}`}
              >
                <ListItemIcon className={classes.listItemIcon}>
                  <Checkbox
                    edge="start"
                    checked={checked}
                    disabled={isMember}
                    tabIndex={-1}
                    disableRipple
                    onClick={(e) => e.stopPropagation()}
                    onChange={() => !isMember && onToggle(u.userId)}
                  />
                </ListItemIcon>

                <ListItemText
                  id={labelId}
                  primary={
                    <Stack direction="row" gap={1} alignItems="center">
                      <Typography>{getFullName(u)}</Typography>
                      <RoleChip role={u.role} data-testid="users-role-badge" />
                    </Stack>
                  }
                  secondary={u.email}
                />
              </ListItemButton>
            </ListItem>
          );
        })}

        {users.length === 0 && (
          <Box className={classes.emptyBox}>
            <Typography variant="body2" className={classes.emptyText}>
              {t("pages.classes.noUsersMatch")}
            </Typography>
          </Box>
        )}
      </List>
    </Box>
  );
};
