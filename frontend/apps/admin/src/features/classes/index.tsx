import { useMemo, useState } from "react";
import {
  Box,
  Button,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Toolbar,
  Typography,
  Tooltip,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import GroupIcon from "@mui/icons-material/Group";
import DeleteIcon from "@mui/icons-material/Delete";
import {
  useGetAllClasses,
  useDeleteClass,
  type ClassSummary,
} from "@admin/api";
import { CreateClassDialog, ManageMembersDialog } from "./components";
import { useStyles } from "./style";

export const Classes = () => {
  const classes = useStyles();
  const { data, isLoading, isError } = useGetAllClasses();
  const { mutate: deleteClass } = useDeleteClass();

  const [createOpen, setCreateOpen] = useState(false);
  const [manageOpen, setManageOpen] = useState(false);
  const [selectedClass, setSelectedClass] = useState<ClassSummary | null>(null);

  const rows = useMemo(() => data ?? [], [data]);

  return (
    <Box className={classes.container}>
      <Toolbar className={classes.toolbar}>
        <Typography variant="h5" className={classes.title}>
          Classes
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setCreateOpen(true)}
        >
          Create Class
        </Button>
      </Toolbar>

      <TableContainer component={Paper} className={classes.tableContainer}>
        <Table>
          <TableHead className={classes.tableHead}>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Members</TableCell>
              <TableCell className={classes.actionsCell}>Actions</TableCell>
            </TableRow>
          </TableHead>

          <TableBody className={classes.tableBody}>
            {isLoading && (
              <TableRow>
                <TableCell colSpan={3} className={classes.loadingCell}>
                  Loading…
                </TableCell>
              </TableRow>
            )}

            {isError && (
              <TableRow>
                <TableCell colSpan={3} className={classes.errorCell}>
                  Failed to load classes
                </TableCell>
              </TableRow>
            )}

            {!isLoading && rows.length === 0 && (
              <TableRow>
                <TableCell colSpan={3} className={classes.emptyCell}>
                  No classes yet
                </TableCell>
              </TableRow>
            )}

            {rows.map((cls) => (
              <TableRow key={cls.classId} className={classes.row} hover>
                <TableCell>{cls.name}</TableCell>
                <TableCell>{cls.members?.length ?? "—"}</TableCell>
                <TableCell className={classes.actionsCell}>
                  <Tooltip title="Manage members">
                    <IconButton
                      className={classes.iconButton}
                      onClick={() => {
                        setSelectedClass(cls);
                        setManageOpen(true);
                      }}
                    >
                      <GroupIcon />
                    </IconButton>
                  </Tooltip>

                  <Tooltip title="Delete class">
                    <span>
                      <IconButton
                        className={classes.iconButton}
                        onClick={() => deleteClass({ classId: cls.classId })}
                      >
                        <DeleteIcon />
                      </IconButton>
                    </span>
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <CreateClassDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
      />

      {selectedClass && (
        <ManageMembersDialog
          open={manageOpen}
          classId={selectedClass.classId}
          className={selectedClass.name}
          onClose={() => setManageOpen(false)}
        />
      )}
    </Box>
  );
};
