import React, { useCallback, useState } from 'react';
import {
  ArrayLayoutProps,
  ArrayTranslations,
  RankedTester,
  isObjectArrayControl,
  isPrimitiveArrayControl,
  or,
  rankWith,
} from '@jsonforms/core';
import {
  withArrayTranslationProps,
  withJsonFormsArrayLayoutProps,
  withTranslateProps,
} from '@jsonforms/react';
import { DeleteDialog } from '@jsonforms/material-renderers';
import { MaterialDataGridControl } from './MaterialDataGridControl';

export const MaterialDataGridControlRenderer = (
  props: ArrayLayoutProps & { translations: ArrayTranslations },
) => {
  const [open, setOpen] = useState(false);
  const [path, setPath] = useState<string | undefined>(undefined);
  const [rowData, setRowData] = useState<number | undefined>(undefined);
  const { removeItems, visible, translations } = props;

  const openDeleteDialog = useCallback(
    (p: string, rowIndex: number) => {
      setOpen(true);
      setPath(p);
      setRowData(rowIndex);
    },
    [setOpen, setPath, setRowData],
  );
  const deleteCancel = useCallback(() => setOpen(false), [setOpen]);
  const deleteConfirm = useCallback(() => {
    const p = path?.substring(0, path.lastIndexOf('.'));
    removeItems!(p as string, [rowData as number])();
    setOpen(false);
  }, [path, removeItems, rowData]);
  const deleteClose = useCallback(() => setOpen(false), [setOpen]);

  if (!visible) {
    return null;
  }

  return (
    <React.Fragment>
      <MaterialDataGridControl
        {...props}
        openDeleteDialog={openDeleteDialog}
        translations={translations}
      />
      <DeleteDialog
        open={open}
        onCancel={deleteCancel}
        onConfirm={deleteConfirm}
        onClose={deleteClose}
        acceptText={translations.deleteDialogAccept as string}
        declineText={translations.deleteDialogDecline as string}
        title={translations.deleteDialogTitle as string}
        message={translations.deleteDialogMessage as string}
      />
    </React.Fragment>
  );
};

export const materialDataGridControlTester: RankedTester = rankWith(
  5,
  or(isObjectArrayControl, isPrimitiveArrayControl),
);

export const MuiDGControl = withJsonFormsArrayLayoutProps(
  withTranslateProps(
    withArrayTranslationProps(MaterialDataGridControlRenderer),
  ),
);

export default {
  tester: materialDataGridControlTester,
  renderer: MuiDGControl,
};
