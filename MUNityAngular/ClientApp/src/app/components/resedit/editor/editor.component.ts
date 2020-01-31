import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { interval, Subscription } from 'rxjs';
import { ResolutionService } from '../../../services/resolution.service';
import { Resolution } from '../../../models/resolution.model';
import { OperativeSection } from 'src/app/models/operative-section.model';
import { NotifierService } from 'angular-notifier';
import { AbstractAmendment } from '../../../models/abstract-amendment.model';
import { AmendmentInspector } from '../../../models/amendment-inspector';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class EditorComponent implements OnInit {

  @Input('resolution')
  public set resolution(v: Resolution) {
    if (v != null) {
      this.model = v;
      if (v.OperativeSections.length > 0) {
        this.amendmentTargetSection = v.OperativeSections[0];
      }
      console.log(this.model);
    }
  }


  public amendmentInspector: AmendmentInspector = new AmendmentInspector();

  


  public get resolution(): Resolution {
    return this.model;
  }

  public amendmentModalActive = false;

  amendmentTargetSection: OperativeSection;

  newAmendmentNewText: string;

  addAmendmentType = "delete";

  amendmentTargetPosition: number;

  constructor(private service: ResolutionService, private route: ActivatedRoute, private notifier: NotifierService,
    private titleService: Title) {
    this.titleService.setTitle('ResaOnline')
  }

  public model: Resolution;

  saveSubscription: Subscription;

  ngOnInit() {
    let id: string = null;
    this.route.params.subscribe(params => {
      id = params.id;
    })
    if (id == null) {
      id = this.route.snapshot.queryParamMap.get('id');
    }

    if (id != null) {
      console.log('Suche resolution mit der ID: ' + id);
      this.service.getResolution(id).subscribe(n => {
        let readyState = this.service.connectionReady;
        //this.service.OrderAmendments(n);
        this.model = n;
        this.service.subscribeToResolution(this.model.ID);
        this.service.addResolutionListener(this.model, this.amendmentInspector);
        this.amendmentInspector.allAmendments = this.service.OrderAmendments(this.model);

        if (n.OperativeSections.length > 0) {
          this.amendmentTargetSection = n.OperativeSections[0];
          this.newAmendmentNewText = n.OperativeSections[0].Text;
        }

        this.titleService.setTitle(this.model.Topic);

        //TODO: Remove this logging
        //console.log(this.model);
      });
    }
  }

  addPreambleParagraph() {
    console.log('Request addPreambleParagraph at: ' + this.model.ID);
    this.service.addPreambleParagraph(this.model.ID).subscribe(data => { }, err => {
      if (err.status == 404) {
        this.notifier.notify('error', 'Ohh nein - Es besteht keine Verbindung zum Server oder die Resolution exisitert nicht.');
      }
      else {
        this.notifier.notify('error', 'Das hat aus unbekannten Gründen nicht geklappt');
      }
      console.log(err)
    });
  }

  openAddAmendmentModal() {
    this.amendmentModalActive = true;
  }

  closeAddAmendmentModal() {
    this.amendmentModalActive = false;
  }

  addAmendmentTypeSelected(newValue) {
    console.log(newValue);
    this.addAmendmentType = newValue;
  }

  addAmendmentTargetSelected(target) {
    this.newAmendmentNewText = this.amendmentTargetSection.Text;
  }

  addAmendmentTargetPositionSelected(target) {
    console.log('position: ' + this.amendmentTargetPosition);
  }

  addOperativeParagraph() {
    this.service.addOperativeParagraph(this.model.ID).subscribe(data => { console.log('Erfolg!') }, err => {
      if (err.status == 404) {
        this.notifier.notify('error', 'Ohh nein - Es besteht keine Verbindung zum Server oder die Resolution exisitert nicht.');
      }
      else {
        this.notifier.notify('error', 'Das hat aus unbekannten Gründen nicht geklappt');
      }
      console.log(err)
    });
  }

  addDeleteAmendment() {
    const resolutionid = this.model.ID;
    const sectionid = this.model.OperativeSections[0].ID;
    this.service.addDeleteAmendment(resolutionid, sectionid, 'Unknown');
  }

  createNewAmendment() {
    const type: string = this.addAmendmentType;
    const target: OperativeSection = this.amendmentTargetSection;
    const newText: string = this.newAmendmentNewText;

    if (type === 'delete') {
      this.service.addDeleteAmendment(this.model.ID, target.ID, 'Unknown');
    } else if (type === 'change') {
      this.service.addChangeAmendment(this.model.ID, target.ID, 'Unknown', newText);
    } else if (type === 'move') {
      this.service.addMoveAmendment(this.model.ID, target.ID, 'Unknown', this.amendmentTargetPosition);
    }

    this.amendmentModalActive = false;
  }
}
